using Enterprise.ApplicationServices.Core.Queries.Handlers.Bound.Logic;
using Enterprise.ApplicationServices.DI.Queries.Handlers.Standard.Bound;
using Enterprise.DI.Core.Registration.Abstract;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.DoesUserHavePassword;

internal sealed class DoesUserHavePasswordQueryServiceRegistrar : IRegisterServices
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddTransient<IQueryLogic<DoesUserHavePasswordQuery, bool>, DoesUserHavePasswordQueryLogic>();
        services.RegisterSimpleQueryHandler<DoesUserHavePasswordQuery, bool>();
    }
}