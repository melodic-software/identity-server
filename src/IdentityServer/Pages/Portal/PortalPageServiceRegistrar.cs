using Enterprise.DI.Core.Registration.Abstract;

namespace IdentityServer.Pages.Portal;

internal sealed class PortalPageServiceRegistrar : IRegisterServices
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddTransient<ClientRepository>();
    }
}
