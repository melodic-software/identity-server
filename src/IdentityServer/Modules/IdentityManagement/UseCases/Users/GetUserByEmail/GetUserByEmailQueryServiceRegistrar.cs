using Enterprise.ApplicationServices.Core.Queries.Handlers.Bound.Logic;
using Enterprise.ApplicationServices.DI.Queries.Handlers.Standard.Bound;
using Enterprise.DI.Core.Registration.Abstract;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.GetUserByEmail;

internal sealed class GetUserByEmailQueryServiceRegistrar : IRegisterServices
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddTransient<IQueryLogic<GetUserByEmailQuery, Result<User>>>(provider =>
        {
            UserManager<ApplicationUser> userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

            return new GetUserByEmailQueryLogic(userManager);
        });

        services.RegisterSimpleQueryHandler<GetUserByEmailQuery, Result<User>>();
    }
}