namespace FiapGames.Notifications.Services;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(Guid userId, string recipientEmail, string name, CancellationToken cancellationToken = default);
    Task SendPurchaseConfirmationAsync(Guid orderId, Guid userId, Guid gameId, CancellationToken cancellationToken = default);
    Task SendPurchaseRejectionAsync(Guid orderId, Guid userId, Guid gameId, CancellationToken cancellationToken = default);
}
