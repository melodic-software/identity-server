using Duende.IdentityServer.EntityFramework.DbContexts;
using IdentityServer.AspNetIdentity.EntityFramework.DbContexts;
using IdentityServer.Constants;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Startup.Migrations;

public static class DatabaseMigrationService
{
    public static async Task MigrateDatabasesAsync(string[] args, WebApplication app)
    {
        ILoggerFactory loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        ILogger logger = loggerFactory.CreateLogger(nameof(DatabaseMigrationService));

        bool applyMigrations = !app.Environment.IsProduction() ||
                               args.Contains("/migrate") ||
                               app.Configuration.GetValue(ConfigurationKeys.MigrateDatabase, false);

        if (!applyMigrations)
        {
            logger.LogInformation("Database migration has been disabled.");
            return;
        }

        logger.LogInformation("Migrating databases...");

        IServiceScopeFactory serviceScopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
        using IServiceScope scope = serviceScopeFactory.CreateAsyncScope();

        await scope.ServiceProvider
            .GetRequiredService<PersistedGrantDbContext>()
            .Database.MigrateAsync();

        await scope.ServiceProvider
            .GetRequiredService<ConfigurationDbContext>()
            .Database.MigrateAsync();

        await scope.ServiceProvider
            .GetRequiredService<AspNetIdentityDbContext>()
            .Database.MigrateAsync();

        logger.LogInformation("Database migration complete.");
    }
}