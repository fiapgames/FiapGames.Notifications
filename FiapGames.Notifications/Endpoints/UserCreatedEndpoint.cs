using System.Text.Json;
using FiapGames.Contracts.IntegrationEvents;
using FiapGames.Notifications.Serialization;
using FiapGames.Notifications.Services;

namespace FiapGames.Notifications.Endpoints;

public static class UserCreatedEndpoint
{
    public static void MapUserCreated(this IEndpointRouteBuilder routes) =>
        routes.MapPost("/notifications/user-created", UserCreated);

    private static async Task<IResult> UserCreated(
        HttpRequest req, INotificationService notifications, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(typeof(UserCreatedEndpoint));
        UserCreatedEvent? userCreated;

        try
        {
            userCreated = await JsonSerializer.DeserializeAsync<UserCreatedEvent>(req.Body, NotificationJson.Default);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Corpo inválido recebido em user-created");
            return Results.BadRequest(new { error = "JSON inválido.", detail = ex.Message });
        }

        if (userCreated is null)
        {
            return Results.BadRequest(new { error = "Corpo da requisição vazio." });
        }

        logger.LogInformation("Received UserCreatedEvent for user {UserId} ({Email})",
            userCreated.UserId, userCreated.Email);

        notifications.NotifyWelcome(userCreated.UserId, userCreated.Email, userCreated.Name);

        return Results.Accepted();
    }
}
