# Roadmap — como esta Function foi parar no Azure

Registro enxuto do caminho até a publicação, incluindo os erros reais e o que os causou.

---

## Ponto de partida

Microsserviço ASP.NET Core com MassTransit/RabbitMQ, PostgreSQL (EF Core) e SMTP (MailHog), rodando em Docker Compose e Kubernetes.

## Chegada

Azure Functions serverless, .NET 10 isolated, três endpoints HTTP, sem banco, sem broker, sem SMTP.

**URL:** `https://fiap-games-notification-hdaza0g6fudjc3ak.westus3-01.azurewebsites.net`

---

## As 3 tentativas (e por que 2 falharam)

| # | Abordagem | Desfecho |
|---|---|---|
| 1 | Consumers como `RabbitMQTrigger` | ❌ O binding **não é suportado** em Consumption/Flex. Exigiria plano B1 (~US$ 12,41/mês). |
| 2 | Broker CloudAMQP via Azure Marketplace | ❌ `Purchase failed`. Conta em free trial não fecha compra no Marketplace, e crédito Azure não cobre terceiros. |
| 3 | **HTTP triggers, sem broker** | ✅ Roda no Consumption gratuito que já existia. |

---

## Passos executados

### 1. Refatoração do código
- Removidos MassTransit/RabbitMQ, EF Core/Npgsql, `SmtpClient`/MailHog, Swashbuckle
- Removidos `Dockerfile`, `compose.yaml`, `k8s/`
- Convertido para Azure Functions v4 isolated (.NET 10)
- 3 endpoints: `GET /api/health`, `POST /api/notifications/user-created`, `POST /api/notifications/payment-processed`
- `NotificationService` preserva assunto e corpo idênticos aos e-mails originais, mas via `ILogger`

### 2. Infraestrutura
O Function App `fiap-games-notification` já existia e já estava correto (Windows, `dotnet-isolated`, `~4`, .NET 10, App Insights).

Foi preciso criar **um** recurso:

```bash
az provider register --namespace Microsoft.Storage
az storage account create -g fiap-games -n stfiapgamesnotif01 \
  -l westus3 --sku Standard_LRS --kind StorageV2

az functionapp config appsettings set -g fiap-games -n fiap-games-notification \
  --settings AzureWebJobsStorage="<connection-string>" \
             WEBSITE_CONTENTAZUREFILECONNECTIONSTRING="<connection-string>" \
             WEBSITE_CONTENTSHARE="fiap-games-notification-content"
```

### 3. Publicação
```bash
dotnet publish FiapGames.Notifications -c Release -o ./publish
cd publish && zip -r ../app.zip . && cd ..
az functionapp deployment source config-zip -g fiap-games -n fiap-games-notification --src app.zip
az functionapp restart -g fiap-games -n fiap-games-notification
```

> O `restart` não é opcional. Após trocar app settings ou o content share, o host só carrega o novo pacote reiniciando.

---

## Os erros que apareceram, e a causa de cada um

### `HTTP 500.30 — ASP.NET Core app failed to start` (1ª vez)

**Causa: o Function App foi criado sem storage account.** Não havia nenhum na subscription inteira.

O host do Azure Functions não sobe sem `AzureWebJobsStorage` — é requisito obrigatório em qualquer plano. Por isso o app aparecia `Running` no portal mas nunca executou nada.

### `SubscriptionNotFound` ao criar o storage

Mensagem enganosa: a subscription estava `Enabled`. A causa real era o resource provider `Microsoft.Storage` em estado `NotRegistered`. Resolvido com `az provider register`.

### `HTTP 500.30` (2ª vez, já com storage)

**Causa: incompatibilidade de versão de pacote no meu código.**

```
System.TypeLoadException: Could not load type
'Microsoft.ApplicationInsights.Extensibility.ITelemetryInitializer'
from assembly 'Microsoft.ApplicationInsights, Version=3.1.2.115'
```

Eu referenciei `Microsoft.ApplicationInsights.WorkerService` **3.1.2** (a mais recente), mas o `Microsoft.Azure.Functions.Worker.ApplicationInsights` espera o SDK **2.x** — o tipo `ITelemetryInitializer` mudou de lugar na 3.x. O host morria no boot com `0 functions loaded`.

**Correção:** fixar em `Microsoft.ApplicationInsights.WorkerService` **2.23.0**.

> Lição: nos pacotes de Application Insights para Functions isolated, "a versão mais nova" não é a versão certa. As duas bibliotecas precisam ser da mesma geração.

### Logs de `Information` não apareciam no Application Insights

O provider de App Insights do worker isolado registra um filtro padrão que descarta tudo abaixo de `Warning`. A correção documentada é remover essa regra no `Program.cs` — já aplicada.

---

## Estado atual

| Item | Status |
|---|---|
| `GET /api/health` | ✅ `200 Healthy` |
| `POST` user-created | ✅ `202 Accepted` |
| `POST` payment-processed (Approved/Rejected) | ✅ `202 Accepted` |
| Sem function key | ✅ `401` |
| JSON inválido | ✅ `400` com detalhe do erro |
| Invocações no Application Insights | ✅ registradas como `requests` |
| Traces `[NOTIFICAÇÃO]` no App Insights | ⚠️ ainda não confirmados — ver abaixo |

**Pendência conhecida:** as invocações aparecem no Application Insights (`requests`), comprovando que as funções executam, mas as linhas `Information` do `NotificationService` ainda não foram observadas na tabela `traces`. A correção do filtro está aplicada; pode ser latência de ingestão ou sampling. Para ver a saída em tempo real:

```bash
az webapp log tail -g fiap-games -n fiap-games-notification
```

---

## Custos

| Recurso | Custo |
|---|---|
| Function App (plano Y1 Consumption) | US$ 0 — 1 mi de execuções/mês grátis |
| Storage `stfiapgamesnotif01` (Standard LRS) | ~US$ 0,00–0,05/mês |
| Application Insights | ~US$ 0 — 5 GB/mês grátis |

Crédito de US$ 200 permanece intacto.

---

## Se quiser religar com os outros microsserviços

Users e Payments ainda publicam no RabbitMQ e **não conversam mais com este serviço**. Para religar, alguém precisa fazer `POST` nos endpoints — os próprios microsserviços ou um consumidor intermediário. É trabalho nos repositórios deles.
