using Enterprise.ApplicationServices.Core.Commands.Dispatching.Facade;
using Enterprise.Events.Handlers.Abstract.Generic.Base;
using IdentityServer.Modules.IdentityManagement.UseCases.Emails.SendConfirmationEmail;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.RegisterUser;

public class UserRegisteredSendConfirmationEmailEventHandler : EventHandlerBase<UserRegisteredDomainEvent>
{
    private readonly ICommandDispatchFacade _commandDispatchFacade;

    public UserRegisteredSendConfirmationEmailEventHandler(
        ICommandDispatchFacade commandDispatchFacade)
    {
        _commandDispatchFacade = commandDispatchFacade;
    }

    public override async Task HandleAsync(UserRegisteredDomainEvent @event, CancellationToken cancellationToken = default)
    {
        var sendConfirmationEmailCommand = new SendConfirmationEmailCommand(@event.UserId, @event.ReturnUrl);
        await _commandDispatchFacade.DispatchAsync(sendConfirmationEmailCommand, cancellationToken);
    }
}
