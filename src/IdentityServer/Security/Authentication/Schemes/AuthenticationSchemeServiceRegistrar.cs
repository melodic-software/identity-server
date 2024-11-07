using Enterprise.DI.Core.Registration.Abstract;
using IdentityServer.Security.Authentication.Schemes.Abstract;

namespace IdentityServer.Security.Authentication.Schemes;

internal sealed class AuthenticationSchemeServiceRegistrar : IRegisterServices
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddScoped<IAuthenticationSchemeService, AuthenticationSchemeService>();
    }
}
