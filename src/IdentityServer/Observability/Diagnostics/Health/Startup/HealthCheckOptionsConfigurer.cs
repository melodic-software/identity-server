using Enterprise.Applications.AspNetCore.Startup.Options;
using Enterprise.Applications.AspNetCore.Startup.Options.Abstract;
using Enterprise.Applications.AspNetCore.Startup.Options.Extensions;
using IdentityServer.Observability.Diagnostics.Health.Checks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IdentityServer.Observability.Diagnostics.Health.Startup;

internal sealed class HealthCheckOptionsConfigurer : IConfigureAppOptions
{
    public static void Configure(AppOptions options, IConfiguration configuration, IHostEnvironment environment)
    {
        options.ConfigureHealthChecks(healthCheckOptions =>
        {
            healthCheckOptions.AddHealthChecks += builder =>
            {
                builder.AddCheck<DuendeIdentityServerLicenseHealthCheck>(
                    "idsrv-license",
                    failureStatus: HealthStatus.Degraded,
                    tags: ["license"]
                );
            };

            healthCheckOptions.MapHealthChecks += app =>
            {
                app.MapHealthChecks("/license-health", new HealthCheckOptions
                {
                    Predicate = registration => registration.Tags.Contains("license")
                });
            };
        });
    }
}