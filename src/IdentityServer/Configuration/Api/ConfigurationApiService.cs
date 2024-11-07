using Duende.IdentityServer.Configuration;
using Duende.IdentityServer.Configuration.EntityFramework;

namespace IdentityServer.Configuration.Api;

public static class ConfigurationApiService
{
    public const string DcrPolicyName = "DCR";

    public static WebApplicationBuilder AddConfigurationApi(this WebApplicationBuilder builder)
    {
        // https://docs.duendesoftware.com/identityserver/v7/configuration/dcr/installation/
        // Requires a business license or higher.
        builder.Services.AddIdentityServerConfiguration(opt =>
            {
                // TODO: Add this when ready to go live in production.
                //opt.LicenseKey = "";
            }
        ).AddClientConfigurationStore();

        return builder;
    }

    public static void UseConfigurationApi(this WebApplication app)
    {
        // Requires a business license or higher.
        // https://oauth.net/2/dynamic-client-registration
        // https://datatracker.ietf.org/doc/html/rfc7591

        app.MapDynamicClientRegistration()
            .WithName("DCR")
            .RequireAuthorization(DcrPolicyName);
    }
}
