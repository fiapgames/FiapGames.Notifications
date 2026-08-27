namespace FiapGames.Notifications.Services;

public interface INotificationService
{
    void NotifyWelcome(Guid userId, string recipientEmail, string name);
    void NotifyPurchaseConfirmation(Guid orderId, Guid userId, Guid gameId);
    void NotifyPurchaseRejection(Guid orderId, Guid userId, Guid gameId);
}
