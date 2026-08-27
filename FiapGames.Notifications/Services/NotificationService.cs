using Microsoft.Extensions.Logging;

namespace FiapGames.Notifications.Services;

/// <summary>
/// Registra as notificações no log da aplicação — console em execução local,
/// Application Insights quando publicado no Azure.
/// </summary>
public class NotificationService(ILogger<NotificationService> logger) : INotificationService
{
    public void NotifyWelcome(Guid userId, string recipientEmail, string name)
    {
        const string subject = "Bem-vindo ao FiapGames!";
        var body = $"Olá {name},\n\nSeja bem-vindo ao FiapGames! Sua conta foi criada com sucesso.\n\nBons jogos!\nEquipe FiapGames";

        Write("WelcomeEmail", userId, recipientEmail, subject, body);
    }

    public void NotifyPurchaseConfirmation(Guid orderId, Guid userId, Guid gameId)
    {
        const string subject = "Confirmação de Compra - FiapGames";
        var recipientEmail = $"user-{userId}@fiapgames.local";
        var body = $"Olá!\n\nSua compra foi aprovada com sucesso.\n\nPedido: {orderId}\nJogo: {gameId}\n\nBons jogos!\nEquipe FiapGames";

        Write("PurchaseConfirmation", orderId, recipientEmail, subject, body);
    }

    public void NotifyPurchaseRejection(Guid orderId, Guid userId, Guid gameId)
    {
        const string subject = "Compra Não Aprovada - FiapGames";
        var recipientEmail = $"user-{userId}@fiapgames.local";
        var body = $"Olá!\n\nInfelizmente sua compra não foi aprovada.\n\nPedido: {orderId}\nJogo: {gameId}\n\nEntre em contato com seu banco ou tente novamente.\nEquipe FiapGames";

        Write("PurchaseRejection", orderId, recipientEmail, subject, body);
    }

    private void Write(string eventType, Guid correlationId, string recipientEmail, string subject, string body) =>
        logger.LogInformation(
            "[NOTIFICAÇÃO] {EventType} | correlationId={CorrelationId} | para={RecipientEmail} | assunto={Subject}\n{Body}",
            eventType, correlationId, recipientEmail, subject, body);
}
