using FiapGames.Contracts.IntegrationEvents;
using FiapGames.Notifications.Services;
using MassTransit;

namespace FiapGames.Notifications.Consumers;

public class UserCreatedConsumer(IEmailService emailService, ILogger<UserCreatedConsumer> logger)
    : IConsumer<UserCreatedEvent>
{
    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var userCreated = context.Message;

        logger.LogInformation("Received UserCreatedEvent for user {UserId} ({Email})", userCreated.UserId, userCreated.Email);

        await emailService.SendWelcomeEmailAsync(userCreated.UserId, userCreated.Email, userCreated.Name, context.CancellationToken);
    }
}
