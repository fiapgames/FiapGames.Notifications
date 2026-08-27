using System.Text.Json;
using System.Text.Json.Serialization;

namespace FiapGames.Notifications.Serialization;

public static class NotificationJson
{
    /// <remarks>
    /// <c>JsonStringEnumConverter</c> é obrigatório: <c>PaymentStatus</c> trafega
    /// como string (<c>"Approved"</c>) no restante do ecossistema FiapGames, não
    /// como número.
    /// </remarks>
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
