# FiapGames.Notifications

Microsserviço responsável pelo envio de notificações por e-mail na plataforma FiapGames. Ele escuta eventos de integração via RabbitMQ e envia e-mails transacionais via SMTP.

## Arquitetura

- **Framework:** ASP.NET Core 8
- **Mensageria:** RabbitMQ (via MassTransit)
- **Banco de dados:** PostgreSQL (logs de notificações via Entity Framework Core)
- **E-mail:** SMTP (MailHog para desenvolvimento local)

## Eventos Consumidos

| Evento | Fila | Ação |
|---|---|---|
| `UserCreatedEvent` | `notifications-user-created` | Envia e-mail de boas-vindas ao novo usuário |
| `PaymentProcessedEvent` | `notifications-payment-processed` | Envia e-mail de confirmação ou rejeição de compra |

## Executando Localmente

**Pré-requisito:** Docker

```bash
docker compose up -d
```

Isso inicializa:
- RabbitMQ em `localhost:5672` (painel de gerenciamento em `localhost:15672`)
- PostgreSQL em `localhost:5433`
- MailHog SMTP em `localhost:1025` (interface web em `localhost:8025`)
- API de Notificações em `localhost:8081`

Para rodar apenas as dependências e iniciar a API localmente:

```bash
docker compose up rabbitmq postgres mailhog -d
dotnet run --project FiapGames.Notifications
```

A API estará disponível em `http://localhost:5000` com Swagger em `/swagger`.

## Configuração

| Chave | Padrão | Descrição |
|---|---|---|
| `ConnectionStrings__Postgres` | `Host=localhost;Port=5432;...` | String de conexão do PostgreSQL |
| `RabbitMq__Host` | `localhost` | Host do RabbitMQ |
| `RabbitMq__VirtualHost` | `/` | Virtual host do RabbitMQ |
| `RabbitMq__UserName` | `fiapgames-admin` | Usuário do RabbitMQ |
| `RabbitMq__Password` | `FiapGames@Admin123` | Senha do RabbitMQ |
| `Smtp__Host` | `localhost` | Host do servidor SMTP |
| `Smtp__Port` | `1025` | Porta do servidor SMTP |
| `Smtp__From` | `noreply@fiapgames.local` | Endereço de e-mail remetente |

## Kubernetes

Os manifestos estão na pasta `k8s/`.

```bash
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/secret.yaml
kubectl apply -f k8s/mailhog.yaml
kubectl apply -f k8s/deployment.yaml
```

O serviço roda no namespace `fiapgames` usando a imagem `lucasceifador/fiapgames-notifications-api:latest`.

## Health Check

```
GET /health
```
