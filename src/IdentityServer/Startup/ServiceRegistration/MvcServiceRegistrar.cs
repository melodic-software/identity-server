using Enterprise.DI.Core.Registration.Abstract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace IdentityServer.Startup.ServiceRegistration;

internal sealed class MvcServiceRegistrar : IRegisterServices
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

        // This was added so URIs generated for emails can be generated more easily (using the route data).
        // TODO: This will likely break IF the email code is broken out into a separate application or service.

        services.AddScoped(x => {
            IActionContextAccessor actionContextAccessor = x.GetRequiredService<IActionContextAccessor>();
            ActionContext? actionContext = actionContextAccessor.ActionContext;
            IUrlHelperFactory factory = x.GetRequiredService<IUrlHelperFactory>();

            if (actionContext == null)
            {
                throw new InvalidOperationException("Cannot construct IUrlHelper without an action context.");
            }

            return factory.GetUrlHelper(actionContext);
        });
    }
}