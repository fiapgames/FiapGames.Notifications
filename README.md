# FiapGames.Notifications

Microsserviço de notificações da plataforma FiapGames, implementado como **AWS Lambda** (.NET 10, ASP.NET Core Minimal API). Expõe endpoints HTTP que recebem os eventos de integração e registram a notificação correspondente no log.

## Arquitetura

- **Runtime:** AWS Lambda `dotnet10` · uma única função com `Amazon.Lambda.AspNetCoreServer.Hosting`
- **Gatilho:** Lambda Function URL (HTTP)
- **Contratos:** pacote `FiapGames.Contracts` (mesmos eventos usados pelos demais microsserviços)
- **Saída:** log da aplicação — console em execução local, CloudWatch Logs na AWS

Sem banco de dados, sem SMTP e sem broker de mensageria.

## Publicado em

A URL da Function URL é exibida no summary de cada execução do workflow **Deploy Lambda**.

Ver [DEPLOY-ROADMAP.md](DEPLOY-ROADMAP.md) para o histórico da publicação anterior no Azure.

## Endpoints

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| `GET` | `/api/health` | IAM | Health check |
| `POST` | `/api/notifications/user-created` | IAM | Notificação de boas-vindas |
| `POST` | `/api/notifications/payment-processed` | IAM | Confirmação ou rejeição de compra |

### Autenticação (AWS_IAM)

A Function URL usa `AuthType: AWS_IAM`: toda requisição precisa ser assinada com SigV4 (serviço `lambda`). Os pods do EKS que chamam o serviço usam uma role (IRSA ou EKS Pod Identity) com a policy:

```json
{
  "Version": "2012-10-17",
  "Statement": [{
    "Effect": "Allow",
    "Action": ["lambda:InvokeFunctionUrl", "lambda:InvokeFunction"],
    "Resource": "arn:aws:lambda:us-east-2:<account-id>:function:<nome-da-funcao>"
  }]
}
```

Ambos os `POST` respondem `202 Accepted` em caso de sucesso e `400 Bad Request` se o corpo for inválido.

### `POST /api/notifications/user-created`

```json
{
  "userId": "11111111-1111-1111-1111-111111111111",
  "name": "Lucas",
  "email": "lucas@fiapgames.local",
  "createdAt": "2026-08-26T12:00:00Z"
}
```

### `POST /api/notifications/payment-processed`

```json
{
  "orderId": "22222222-2222-2222-2222-222222222222",
  "userId": "11111111-1111-1111-1111-111111111111",
  "gameId": "33333333-3333-3333-3333-333333333333",
  "status": "Approved",
  "processedAt": "2026-08-26T12:00:00Z"
}
```

`status` aceita `"Approved"` ou `"Rejected"` — **como string**, não como número.

## Executando Localmente

```bash
cd FiapGames.Notifications
dotnet run
```

Localmente não há autenticação:

```bash
curl http://localhost:5000/api/health

curl -X POST http://localhost:5000/api/notifications/user-created \
  -H "Content-Type: application/json" \
  -d '{"userId":"11111111-1111-1111-1111-111111111111","name":"Lucas","email":"lucas@fiapgames.local","createdAt":"2026-08-26T12:00:00Z"}'
```

## Deploy (GitHub Actions)

O workflow `.github/workflows/deploy.yml` faz build em todo PR e, em push na `main` (ou execução manual), publica na Lambda. A função é criada pelo console da AWS; o workflow atualiza código e configuração e cria a Function URL se ainda não existir.

Configuração no repositório (Settings → Secrets and variables → Actions; modelo no `.env` local, que não é commitado):

| Nome | Tipo | Valor |
|---|---|---|
| `AWS_ACCESS_KEY_ID` / `AWS_SECRET_ACCESS_KEY` | Secret | Credenciais do usuário IAM de deploy |
| `AWS_REGION` | Variable | `us-east-2` |
| `LAMBDA_FUNCTION_NAME` | Variable | `fiap-games-notification` |

## Estrutura

```
FiapGames.Notifications/
├── Program.cs                  host ASP.NET Core + Lambda hosting + DI
├── appsettings.json            níveis de log
├── Endpoints/
│   ├── HealthEndpoint.cs
│   ├── UserCreatedEndpoint.cs
│   └── PaymentProcessedEndpoint.cs
├── Services/
│   ├── INotificationService.cs
│   └── NotificationService.cs  monta assunto/corpo e registra no log
└── Serialization/
    └── NotificationJson.cs     JsonStringEnumConverter para PaymentStatus
```
