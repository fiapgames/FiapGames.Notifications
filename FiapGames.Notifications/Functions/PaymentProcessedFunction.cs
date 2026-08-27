using System.Text.Json;
using FiapGames.Contracts.IntegrationEvents;
using FiapGames.Notifications.Serialization;
using FiapGames.Notifications.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FiapGames.Notifications.Functions;

public class PaymentProcessedFunction(INotificationService notifications, ILogger<PaymentProcessedFunction> logger)
{
    [Function(nameof(PaymentProcessed))]
    public async Task<IActionResult> PaymentProcessed(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "notifications/payment-processed")] HttpRequest req)
    {
        PaymentProcessedEvent? paymentProcessed;

        try
        {
            paymentProcessed = await JsonSerializer.DeserializeAsync<PaymentProcessedEvent>(req.Body, NotificationJson.Default);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Corpo inválido recebido em payment-processed");
            return new BadRequestObjectResult(new { error = "JSON inválido.", detail = ex.Message });
        }

        if (paymentProcessed is null)
        {
            return new BadRequestObjectResult(new { error = "Corpo da requisição vazio." });
        }

        logger.LogInformation("Received PaymentProcessedEvent for order {OrderId} with status {Status}",
            paymentProcessed.OrderId, paymentProcessed.Status);

        if (paymentProcessed.Status == PaymentStatus.Approved)
        {
            notifications.NotifyPurchaseConfirmation(
                paymentProcessed.OrderId, paymentProcessed.UserId, paymentProcessed.GameId);
        }
        else
        {
            notifications.NotifyPurchaseRejection(
                paymentProcessed.OrderId, paymentProcessed.UserId, paymentProcessed.GameId);
        }

        return new AcceptedResult();
    }
}
