using System.Net.Mail;
using FiapGames.Notifications.Configuration;
using FiapGames.Notifications.Data;
using FiapGames.Notifications.Models;
using Microsoft.Extensions.Options;

namespace FiapGames.Notifications.Services;

public class EmailService(
    NotificationsDbContext dbContext,
    IOptions<SmtpOptions> smtpOptions,
    ILogger<EmailService> logger) : IEmailService
{
    private readonly SmtpOptions _smtp = smtpOptions.Value;

    public async Task SendWelcomeEmailAsync(Guid userId, string recipientEmail, string name, CancellationToken cancellationToken = default)
    {
        const string subject = "Bem-vindo ao FiapGames!";
        var body = $"Olá {name},\n\nSeja bem-vindo ao FiapGames! Sua conta foi criada com sucesso.\n\nBons jogos!\nEquipe FiapGames";

        await SendEmailAsync(recipientEmail, subject, body);

        logger.LogInformation("E-mail de boas-vindas enviado para {Email} (usuário {UserId})", recipientEmail, userId);

        dbContext.NotificationLogs.Add(new NotificationLog
        {
            Id = Guid.NewGuid(),
            EventType = "WelcomeEmail",
            CorrelationId = userId,
            RecipientEmail = recipientEmail,
            Subject = subject,
            SentAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SendPurchaseConfirmationAsync(Guid orderId, Guid userId, Guid gameId, CancellationToken cancellationToken = default)
    {
        const string subject = "Confirmação de Compra - FiapGames";
        var recipientEmail = $"user-{userId}@fiapgames.local";
        var body = $"Olá!\n\nSua compra foi aprovada com sucesso.\n\nPedido: {orderId}\nJogo: {gameId}\n\nBons jogos!\nEquipe FiapGames";

        await SendEmailAsync(recipientEmail, subject, body);

        logger.LogInformation("E-mail de confirmação de compra enviado para UserId={UserId} (pedido {OrderId})", userId, orderId);

        dbContext.NotificationLogs.Add(new NotificationLog
        {
            Id = Guid.NewGuid(),
            EventType = "PurchaseConfirmation",
            CorrelationId = orderId,
            RecipientEmail = recipientEmail,
            Subject = subject,
            SentAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SendPurchaseRejectionAsync(Guid orderId, Guid userId, Guid gameId, CancellationToken cancellationToken = default)
    {
        const string subject = "Compra Não Aprovada - FiapGames";
        var recipientEmail = $"user-{userId}@fiapgames.local";
        var body = $"Olá!\n\nInfelizmente sua compra não foi aprovada.\n\nPedido: {orderId}\nJogo: {gameId}\n\nEntre em contato com seu banco ou tente novamente.\nEquipe FiapGames";

        await SendEmailAsync(recipientEmail, subject, body);

        logger.LogInformation("E-mail de rejeição de compra enviado para UserId={UserId} (pedido {OrderId})", userId, orderId);

        dbContext.NotificationLogs.Add(new NotificationLog
        {
            Id = Guid.NewGuid(),
            EventType = "PurchaseRejection",
            CorrelationId = orderId,
            RecipientEmail = recipientEmail,
            Subject = subject,
            SentAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SendEmailAsync(string to, string subject, string body)
    {
#pragma warning disable SYSLIB0026, SYSLIB0048
        using var client = new SmtpClient(_smtp.Host, _smtp.Port)
        {
            EnableSsl = false,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        var message = new MailMessage(_smtp.From, to, subject, body);
        await client.SendMailAsync(message);
#pragma warning restore SYSLIB0026, SYSLIB0048
    }
}
