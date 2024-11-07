using Enterprise.Middleware.AspNetCore.Registration.Abstract;
using IdentityServer.Configuration.Api;

namespace IdentityServer.Middleware;

internal sealed class MiddlewareRegistrar : IRegisterMiddleware
{
    public static void RegisterMiddleware(WebApplication app)
    {
        app.UseConfigurationApi();
    }
}
