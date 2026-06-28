namespace FiapGames.Notifications.Models;

public class NotificationLog
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}
