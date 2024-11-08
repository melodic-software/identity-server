using Enterprise.ApplicationServices.Core.Commands.Handlers.Pragmatic.Base;
using Enterprise.Events.Facade.Abstract;
using Enterprise.Patterns.ResultPattern.Model;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Ciba;

public class SendBackChannelAuthenticationEmailCommandHandler : CommandHandler<SendBackChannelAuthenticationEmailCommand, Result>
{
    public SendBackChannelAuthenticationEmailCommandHandler(IEventRaisingFacade eventService) : base(eventService)
    {
    }

    public override Task<Result> HandleAsync(SendBackChannelAuthenticationEmailCommand command, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}