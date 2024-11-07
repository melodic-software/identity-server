using Enterprise.ApplicationServices.Core.Queries.Handlers.Bound.Logic;
using Enterprise.ApplicationServices.DI.Queries.Handlers.Standard.Bound;
using Enterprise.DI.Core.Registration.Abstract;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.GetUserByExternalLogin;

internal sealed class GetUserByExternalLoginQueryServiceRegistrar : IRegisterServices
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddTransient<IQueryLogic<GetUserByExternalLoginQuery, Result<User>>>(provider =>
        {
            UserManager<ApplicationUser> userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

            return new GetUserByExternalLoginQueryLogic(userManager);
        });

        services.RegisterSimpleQueryHandler<GetUserByExternalLoginQuery, Result<User>>();
    }
}