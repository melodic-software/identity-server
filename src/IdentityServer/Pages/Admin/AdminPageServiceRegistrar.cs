using Enterprise.DI.Core.Registration.Abstract;
using IdentityServer.Pages.Admin.ApiScopes;
using IdentityServer.Pages.Admin.Clients;
using IdentityServer.Pages.Admin.IdentityScopes;

namespace IdentityServer.Pages.Admin;

internal sealed class AdminPageServiceRegistrar : IRegisterServices
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddTransient<ClientRepository>();
        services.AddTransient<IdentityScopeRepository>();
        services.AddTransient<ApiScopeRepository>();
    }
}
