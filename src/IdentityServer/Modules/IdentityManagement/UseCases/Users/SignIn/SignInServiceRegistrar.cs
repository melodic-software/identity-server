using Enterprise.ApplicationServices.DI.Commands.Handlers.Standard.Pragmatic;
using Enterprise.DI.Core.Registration.Abstract;
using Enterprise.Patterns.ResultPattern.Model.Generic;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.SignIn;

internal sealed class SignInServiceRegistrar : IRegisterServices
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.RegisterCommandHandler<SignInCommand, Result<bool>, SignInCommandHandler>();
        services.RegisterCommandHandler<SignInLocalCommand, Result<SignInLocalResult>, SignInLocalCommandHandler>();
        services.RegisterCommandHandler<SignInExternalCommand, Result<ExternalSignIn>, SignInExternalCommandHandler>();
    }
}
