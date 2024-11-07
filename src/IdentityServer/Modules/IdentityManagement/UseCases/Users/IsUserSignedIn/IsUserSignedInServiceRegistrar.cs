using Enterprise.ApplicationServices.DI.Queries.Handlers.Standard.Bound;
using Enterprise.DI.Core.Registration.Abstract;
using Enterprise.Events.Facade.Abstract;
using IdentityServer.AspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.IsUserSignedIn;

internal sealed class IsUserSignedInServiceRegistrar : IRegisterServices
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.RegisterQueryHandler(provider =>
        {
            IHttpContextAccessor httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
            SignInManager<ApplicationUser> signInManager = provider.GetRequiredService<SignInManager<ApplicationUser>>();
            IEventRaisingFacade eventService = provider.GetRequiredService<IEventRaisingFacade>();

            return new IsUserSignedInQueryHandler(httpContextAccessor, signInManager, eventService);
        });
    }
}