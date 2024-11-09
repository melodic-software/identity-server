using Duende.IdentityServer;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IdentityServer.Observability.Diagnostics.Health.Checks;

/// <summary>
/// This is a health check specifically for the licensing of Duende IdentityServer.
/// https://osmanmelsayed.com/tracking-duende-identity-servers-license-validity
/// </summary>
public class DuendeIdentityServerLicenseHealthCheck : IHealthCheck
{
    private readonly IdentityServerLicense? _license;
    private readonly IWebHostEnvironment _environment;

    public DuendeIdentityServerLicenseHealthCheck(
        IWebHostEnvironment environment,
        IdentityServerLicense? license = null)
    {
        _environment = environment;
        _license = license;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (!_environment.IsProduction())
        {
            string nonProdLicenseMessage = "Duende Identity Server's license is not required if the environment is not production.";
            var nonProdResult = HealthCheckResult.Healthy(nonProdLicenseMessage);
            return Task.FromResult(nonProdResult);
        }

        if (_license == null)
        {
            string invalidLicenseMessage = "Invalid or missing Duende Identity Server's license.";
            var invalidLicenseResult = new HealthCheckResult(context.Registration.FailureStatus, invalidLicenseMessage);
            return Task.FromResult(invalidLicenseResult);
        }

        var healthCheckData = new Dictionary<string, object>();

        if (_license.Expiration != null)
        {
            healthCheckData.Add("Expiration", _license.Expiration);
        }

        if (_license.Expiration < DateTime.UtcNow)
        {
            string expiredLicenseMessage = "Duende Identity Server's license has expired.";

            var expiredLicenseResult = new HealthCheckResult(
                context.Registration.FailureStatus,
                expiredLicenseMessage,
                null,
                healthCheckData
            );

            return Task.FromResult(expiredLicenseResult);
        }

        string healthyMessage = "Duende Identity Server's license is valid.";
        var healthyResult = HealthCheckResult.Healthy(healthyMessage, healthCheckData);
        return Task.FromResult(healthyResult);
    }
}