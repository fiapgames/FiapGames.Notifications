using System.Text.Json;
using FiapGames.Contracts.IntegrationEvents;
using FiapGames.Notifications.Serialization;
using FiapGames.Notifications.Services;

namespace FiapGames.Notifications.Endpoints;

public static class PaymentProcessedEndpoint
{
    public static void MapPaymentProcessed(this IEndpointRouteBuilder routes) =>
        routes.MapPost("/notifications/payment-processed", PaymentProcessed);

    private static async Task<IResult> PaymentProcessed(
        HttpRequest req, INotificationService notifications, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(typeof(PaymentProcessedEndpoint));
        PaymentProcessedEvent? paymentProcessed;

        try
        {
            paymentProcessed = await JsonSerializer.DeserializeAsync<PaymentProcessedEvent>(req.Body, NotificationJson.Default);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Corpo inválido recebido em payment-processed");
            return Results.BadRequest(new { error = "JSON inválido.", detail = ex.Message });
        }

        if (paymentProcessed is null)
        {
            return Results.BadRequest(new { error = "Corpo da requisição vazio." });
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

        return Results.Accepted();
    }
}
