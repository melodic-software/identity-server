using Enterprise.ApplicationServices.Core.Queries.Handlers.Bound.Logic;
using Enterprise.ApplicationServices.DI.Queries.Handlers.Standard.Bound;
using Enterprise.DI.Core.Registration.Abstract;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Passwords.GetPasswordRequirements;

internal sealed class GetPasswordRequirementsQueryRegistrar : IRegisterServices
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddTransient<IQueryLogic<GetPasswordRequirementsQuery, PasswordRequirements>, GetPasswordRequirementsQueryLogic>();
        services.RegisterSimpleQueryHandler<GetPasswordRequirementsQuery, PasswordRequirements>();
    }
}