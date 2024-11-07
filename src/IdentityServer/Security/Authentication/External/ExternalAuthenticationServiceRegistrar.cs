using Enterprise.DI.Core.Registration.Abstract;
using IdentityServer.Security.Authentication.External.Abstract;

namespace IdentityServer.Security.Authentication.External;

internal sealed class ExternalAuthenticationServiceRegistrar : IRegisterServices
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddScoped<IExternalAuthenticationService, ExternalAuthenticationService>();
    }
}
