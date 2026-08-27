using System.Text.Json;
using FiapGames.Contracts.IntegrationEvents;
using FiapGames.Notifications.Serialization;
using FiapGames.Notifications.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FiapGames.Notifications.Functions;

public class UserCreatedFunction(INotificationService notifications, ILogger<UserCreatedFunction> logger)
{
    [Function(nameof(UserCreated))]
    public async Task<IActionResult> UserCreated(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "notifications/user-created")] HttpRequest req)
    {
        UserCreatedEvent? userCreated;

        try
        {
            userCreated = await JsonSerializer.DeserializeAsync<UserCreatedEvent>(req.Body, NotificationJson.Default);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Corpo inválido recebido em user-created");
            return new BadRequestObjectResult(new { error = "JSON inválido.", detail = ex.Message });
        }

        if (userCreated is null)
        {
            return new BadRequestObjectResult(new { error = "Corpo da requisição vazio." });
        }

        logger.LogInformation("Received UserCreatedEvent for user {UserId} ({Email})",
            userCreated.UserId, userCreated.Email);

        notifications.NotifyWelcome(userCreated.UserId, userCreated.Email, userCreated.Name);

        return new AcceptedResult();
    }
}
