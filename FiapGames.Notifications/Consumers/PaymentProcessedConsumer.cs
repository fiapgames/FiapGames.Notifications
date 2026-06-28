using FiapGames.Contracts.IntegrationEvents;
using FiapGames.Notifications.Services;
using MassTransit;

namespace FiapGames.Notifications.Consumers;

public class PaymentProcessedConsumer(IEmailService emailService, ILogger<PaymentProcessedConsumer> logger)
    : IConsumer<PaymentProcessedEvent>
{
    public async Task Consume(ConsumeContext<PaymentProcessedEvent> context)
    {
        var paymentProcessed = context.Message;

        logger.LogInformation("Received PaymentProcessedEvent for order {OrderId} with status {Status}",
            paymentProcessed.OrderId, paymentProcessed.Status);

        if (paymentProcessed.Status == PaymentStatus.Approved)
        {
            await emailService.SendPurchaseConfirmationAsync(
                paymentProcessed.OrderId,
                paymentProcessed.UserId,
                paymentProcessed.GameId,
                context.CancellationToken);
        }
        else
        {
            await emailService.SendPurchaseRejectionAsync(
                paymentProcessed.OrderId,
                paymentProcessed.UserId,
                paymentProcessed.GameId,
                context.CancellationToken);
        }
    }
}
