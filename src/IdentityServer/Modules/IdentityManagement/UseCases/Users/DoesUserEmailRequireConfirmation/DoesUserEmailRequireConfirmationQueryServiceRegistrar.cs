using Enterprise.ApplicationServices.Core.Queries.Handlers.Bound.Logic;
using Enterprise.ApplicationServices.DI.Queries.Handlers.Standard.Bound;
using Enterprise.DI.Core.Registration.Abstract;
using IdentityServer.AspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.DoesUserEmailRequireConfirmation;

internal sealed class DoesUserEmailRequireConfirmationQueryServiceRegistrar : IRegisterServices
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddTransient<IQueryLogic<DoesUserEmailRequireConfirmationQuery, bool>>(provider =>
        {
            UserManager<ApplicationUser> userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

            return new DoesUserEmailRequireConfirmationQueryLogic(userManager);
        });

        services.RegisterSimpleQueryHandler<DoesUserEmailRequireConfirmationQuery, bool>();
    }
}