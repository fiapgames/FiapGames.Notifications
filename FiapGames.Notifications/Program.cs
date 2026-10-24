using FiapGames.Notifications.Endpoints;
using FiapGames.Notifications.Services;

var builder = WebApplication.CreateBuilder(args);

// Na Lambda, o hosting traduz os eventos da Function URL (payload v2, mesmo
// formato da HTTP API) em requisições ASP.NET Core. Fora da Lambda é no-op e
// a aplicação sobe no Kestrel normalmente (dotnet run).
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

builder.Services.AddSingleton<INotificationService, NotificationService>();

var app = builder.Build();

var api = app.MapGroup("/api");
api.MapHealth();
api.MapUserCreated();
api.MapPaymentProcessed();

app.Run();
