using Duende.IdentityServer.EntityFramework.DbContexts;
using IdentityServer.AspNetIdentity.Roles.Seeding;
using IdentityServer.AspNetIdentity.Users.Seeding;
using IdentityServer.Configuration.EntityFramework.Seeding;
using IdentityServer.Constants;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace IdentityServer.Startup.Seeding;

public static class DataSeedService
{
    public static async Task SeedDataAsync(string[] args, WebApplication app)
    {
        bool seedData = !app.Environment.IsProduction() ||
                        args.Contains("/seed") ||
                        app.Configuration.GetValue(ConfigurationKeys.SeedData, false);

        if (!seedData)
        {
            return;
        }

        ILoggerFactory loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        ILogger logger = loggerFactory.CreateLogger(nameof(DataSeedService));

        logger.LogInformation("Seeding databases...");

        IServiceScopeFactory serviceScopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
        using IServiceScope scope = serviceScopeFactory.CreateAsyncScope();

        ConfigurationDbContext configurationDbContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
        await ConfigurationDataSeedService.SeedConfigurationDataAsync(configurationDbContext, scope.ServiceProvider);

        await RoleSeedService.SeedRolesAsync(scope);
        await UserSeedService.SeedAdminUserAsync(scope, app.Configuration);
        //await TestUserSeedService.SeedTestUsersAsync(scope, app.Environment);

        logger.LogInformation("Database seeding complete.");
    }
}
