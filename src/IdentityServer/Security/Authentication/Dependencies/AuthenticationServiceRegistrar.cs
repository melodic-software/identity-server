using Enterprise.DI.Core.Registration.Abstract;
using IdentityServer.Constants;
using IdentityServer.Security.Authentication.External;
using IdentityServer.Security.Authentication.External.Abstract;
using IdentityServer.Security.Authentication.Schemes;
using IdentityServer.Security.Authentication.Schemes.Abstract;
using Microsoft.AspNetCore.Authentication;

namespace IdentityServer.Security.Authentication.Dependencies;

internal sealed class AuthenticationServiceRegistrar : IRegisterServices
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddScoped<IAuthenticationSchemeService, AuthenticationSchemeService>();
        services.AddScoped<IExternalAuthenticationService, ExternalAuthenticationService>();

        // We are using ASP.NETs "Identity.Application" and "Identity.External" cookie
        // instead of the built-in "idsrv" and "idsrv.external" cookie schemes provided by Identity Server.
        services.PostConfigure<AuthenticationOptions>(options =>
        {
            options.DefaultScheme = CookieSchemes.CookieAuthenticationScheme;
            options.DefaultSignInScheme = CookieSchemes.ExternalCookieAuthenticationScheme;
        });
    }
}
