using Enterprise.ApplicationServices.DI.Commands.Handlers.Standard.Pragmatic;
using Enterprise.DI.Core.Registration.Abstract;
using Enterprise.Patterns.ResultPattern.Model;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.SignOut;

internal sealed class SignOutServiceRegistrar : IRegisterServices
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.RegisterCommandHandler<SignOutCommand, Result, SignOutCommandHandler>();
    }
}