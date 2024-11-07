using Enterprise.Applications.AspNetCore.Startup.Events;
using Enterprise.Applications.AspNetCore.Startup.Events.Abstract;
using IdentityServer.Configuration.Api;
using IdentityServer.Security.KeyVaults;
using IdentityServer.Startup.Migrations;
using IdentityServer.Startup.Seeding;

namespace IdentityServer.Startup.Events;

internal sealed class AppConfigEventHandlerRegistrar : IRegisterAppConfigEventHandlers
{
    public static void RegisterHandlers(AppConfigEvents events)
    {
        string[] arguments = [];

        events.ConfigurationStarted += args =>
        {
            arguments = args;

            // In .NET 5 and later versions, the Activity.DefaultIdFormat is ActivityIdFormat.W3C.
            // In previous versions, the default format is ActivityIdFormat.Hierarchical.
            // To make use of the Activity's traces and spans, this needs to be to ActivityIdFormat.W3C.
            // https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/5.0/default-activityidformat-changed
            // https://docs.duendesoftware.com/identityserver/v5/diagnostics/logging/
            //Activity.DefaultIdFormat = ActivityIdFormat.W3C;

            return Task.CompletedTask;
        };

        events.BuilderCreated += builder =>
        {
            // TODO: The key vault can eventually be handled and configured in the enterprise package.
            builder.AddAzureKeyVault();
            builder.AddConfigurationApi();
            return Task.CompletedTask;
        };

        events.WebApplicationBuilt += app =>
        {
            return Task.CompletedTask;
        };

        events.RequestPipelineConfigured += async app =>
        {
            await DatabaseMigrationService.MigrateDatabasesAsync(arguments, app);
            await DataSeedService.SeedDataAsync(arguments, app);
        };
    }
}
