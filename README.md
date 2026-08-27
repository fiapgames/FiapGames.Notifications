# FiapGames.Notifications

Microsserviço de notificações da plataforma FiapGames, implementado como **Azure Functions serverless** (.NET 10 isolated). Expõe endpoints HTTP que recebem os eventos de integração e registram a notificação correspondente no log.

## Arquitetura

- **Runtime:** Azure Functions v4 · .NET 10 · modelo *isolated worker*
- **Gatilhos:** HTTP
- **Contratos:** pacote `FiapGames.Contracts` (mesmos eventos usados pelos demais microsserviços)
- **Saída:** log da aplicação — console em execução local, Application Insights no Azure

Sem banco de dados, sem SMTP e sem broker de mensageria.

## Publicado em

```
https://fiap-games-notification-hdaza0g6fudjc3ak.westus3-01.azurewebsites.net
```

Ver [DEPLOY-ROADMAP.md](DEPLOY-ROADMAP.md) para o histórico da publicação.

## Endpoints

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| `GET` | `/api/health` | Anônimo | Health check |
| `POST` | `/api/notifications/user-created` | Function key | Notificação de boas-vindas |
| `POST` | `/api/notifications/payment-processed` | Function key | Confirmação ou rejeição de compra |

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

**Pré-requisito:** [Azure Functions Core Tools v4](https://learn.microsoft.com/azure/azure-functions/functions-run-local)

```bash
cd FiapGames.Notifications
func start
```

Os endpoints sobem em `http://localhost:7071`. Em execução local a *function key* não é exigida:

```bash
curl http://localhost:7071/api/health

curl -X POST http://localhost:7071/api/notifications/user-created \
  -H "Content-Type: application/json" \
  -d '{"userId":"11111111-1111-1111-1111-111111111111","name":"Lucas","email":"lucas@fiapgames.local","createdAt":"2026-08-26T12:00:00Z"}'
```

## Publicando no Azure

```bash
dotnet publish FiapGames.Notifications -c Release -o ./publish
cd publish && zip -r ../app.zip . && cd ..

az functionapp deployment source config-zip \
  -g fiap-games -n fiap-games-notification --src app.zip
```

Recuperar a *function key* para chamar os endpoints protegidos:

```bash
az functionapp function keys list \
  -g fiap-games -n fiap-games-notification --function-name UserCreated
```

E então incluir no header `x-functions-key`.

## Estrutura

```
FiapGames.Notifications/
├── Program.cs                  host isolated + DI + Application Insights
├── host.json                   routePrefix e logging
├── Functions/
│   ├── HealthFunction.cs
│   ├── UserCreatedFunction.cs
│   └── PaymentProcessedFunction.cs
├── Services/
│   ├── INotificationService.cs
│   └── NotificationService.cs  monta assunto/corpo e registra no log
└── Serialization/
    └── NotificationJson.cs     JsonStringEnumConverter para PaymentStatus
```
