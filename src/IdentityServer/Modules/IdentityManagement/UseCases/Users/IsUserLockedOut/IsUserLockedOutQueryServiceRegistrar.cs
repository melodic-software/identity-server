using Enterprise.ApplicationServices.Core.Queries.Handlers.Bound.Logic;
using Enterprise.ApplicationServices.DI.Queries.Handlers.Standard.Bound;
using Enterprise.DI.Core.Registration.Abstract;
using IdentityServer.AspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.IsUserLockedOut;

internal sealed class IsUserLockedOutQueryServiceRegistrar : IRegisterServices
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddTransient<IQueryLogic<IsUserLockedOutQuery, bool>>(provider =>
        {
            UserManager<ApplicationUser> userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

            return new IsUserLockedOutQueryLogic(userManager);
        });

        services.RegisterSimpleQueryHandler<IsUserLockedOutQuery, bool>();
    }
}