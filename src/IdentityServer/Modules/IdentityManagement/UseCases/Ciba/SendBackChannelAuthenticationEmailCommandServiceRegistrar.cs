using Enterprise.ApplicationServices.DI.Commands.Handlers.Standard.Pragmatic;
using Enterprise.DI.Core.Registration.Abstract;
using Enterprise.Patterns.ResultPattern.Model;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Ciba;

public class SendBackChannelAuthenticationEmailCommandServiceRegistrar : IRegisterServices
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.RegisterCommandHandler<SendBackChannelAuthenticationEmailCommand, Result, SendBackChannelAuthenticationEmailCommandHandler>();
    }
}
