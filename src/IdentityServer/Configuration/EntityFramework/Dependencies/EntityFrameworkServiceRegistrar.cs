using Enterprise.DI.Core.Registration.Abstract;

namespace IdentityServer.Configuration.EntityFramework.Dependencies;

internal sealed class EntityFrameworkServiceRegistrar : IRegisterServices
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            // In combination with UseDeveloperExceptionPage, this captures database-related exceptions
            // that can be resolved by using Entity Framework migrations. When these exceptions occur,
            // an HTML response with details about possible actions to resolve the issue is generated.
            services.AddDatabaseDeveloperPageExceptionFilter();
        }
    }
}
