using Enterprise.ApplicationServices.Core.Queries.Handlers.Bound.Logic;
using Enterprise.ApplicationServices.DI.Queries.Handlers.Standard.Bound;
using Enterprise.DI.Core.Registration.Abstract;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.GetLoggedInUser;

internal sealed class GetLoggedInUserQueryServiceRegistrar : IRegisterServices
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddTransient<IQueryLogic<GetLoggedInUserQuery, Result<User>>, GetLoggedInUserQueryLogic>();
        services.RegisterSimpleQueryHandler<GetLoggedInUserQuery, Result<User>>();
    }
}