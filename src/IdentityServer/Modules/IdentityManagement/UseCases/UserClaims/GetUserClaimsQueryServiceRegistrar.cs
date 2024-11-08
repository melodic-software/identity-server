using Enterprise.ApplicationServices.Core.Queries.Handlers.Bound.Logic;
using Enterprise.ApplicationServices.DI.Queries.Handlers.Standard.Bound;
using Enterprise.DI.Core.Registration.Abstract;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using System.Security.Claims;

namespace IdentityServer.Modules.IdentityManagement.UseCases.UserClaims;

internal sealed class GetUserClaimsQueryServiceRegistrar : IRegisterServices
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddTransient<IQueryLogic<GetUserClaimsQuery, Result<ICollection<Claim>>>, GetUserClaimsQueryLogic>();
        services.RegisterSimpleQueryHandler<GetUserClaimsQuery, Result<ICollection<Claim>>>();
    }
}