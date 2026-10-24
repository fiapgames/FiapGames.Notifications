namespace FiapGames.Notifications.Endpoints;

public static class HealthEndpoint
{
    public static void MapHealth(this IEndpointRouteBuilder routes) =>
        routes.MapGet("/health", () => Results.Text("Healthy"));
}
