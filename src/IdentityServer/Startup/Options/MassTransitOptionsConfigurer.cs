using Enterprise.Applications.AspNetCore.Startup.Options;
using Enterprise.Applications.AspNetCore.Startup.Options.Abstract;
using Enterprise.Applications.AspNetCore.Startup.Options.Extensions;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.RegisterUser;

namespace IdentityServer.Startup.Options;

internal sealed class MassTransitOptionsConfigurer : IConfigureAppOptions
{
    public static void Configure(AppOptions options, IConfiguration configuration, IHostEnvironment environment)
    {
        options.ConfigureMassTransit(massTransitOptions =>
        {
            massTransitOptions.AddAssembly(AssemblyReference.Assembly);

            massTransitOptions.AddRegistration(configurator =>
            {
                configurator.AddConsumer<UserRegisteredIntegrationEventConsumer>();
            });
        });
    }
}
