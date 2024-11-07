using MassTransit;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.RegisterUser;

// This is just a demo. The consumer would live in another project.
// TODO: Remove this once done testing MassTransit consumers.

public class UserRegisteredIntegrationEventConsumer : IConsumer<UserRegisteredIntegrationEvent>
{
    private readonly ILogger<UserRegisteredIntegrationEventConsumer> _logger;

    public UserRegisteredIntegrationEventConsumer(ILogger<UserRegisteredIntegrationEventConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<UserRegisteredIntegrationEvent> context)
    {
        _logger.LogInformation("Message consumed: {MessageTypeName}", nameof(UserRegisteredIntegrationEvent));
        _logger.LogInformation("{@UserRegisteredIntegrationEvent}", context.Message);
        _logger.LogInformation("{@ConsumeContext}", context);
        return Task.CompletedTask;
    }
}