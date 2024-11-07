using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using IdentityServer.Configuration.InMemory;
using IdentityServer.Startup.Seeding;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Configuration.EntityFramework.Seeding;

public static class ConfigurationDataSeedService
{
    public static async Task SeedConfigurationDataAsync(ConfigurationDbContext context, IServiceProvider serviceProvider)
    {
        ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        ILogger logger = loggerFactory.CreateLogger(nameof(DataSeedService));

        logger.LogDebug("Clients being populated.");

        //List<string> existingClientIds = await context.Clients.Select(x => x.ClientId).ToListAsync();

        //foreach (Client client in ConfigurationData.Clients.ToList())
        //{
        //    if (existingClientIds.Contains(client.ClientId))
        //    {
        //        continue;
        //    }

        //    context.Clients.Add(client.ToEntity());
        //}

        if (!(await context.IdentityResources.AnyAsync()))
        {
            logger.LogDebug("IdentityResources being populated.");

            foreach (IdentityResource resource in ConfigurationData.IdentityResources.ToList())
            {
                context.IdentityResources.Add(resource.ToEntity());
            }

            await context.SaveChangesAsync();
        }
        else
        {
            logger.LogDebug("IdentityResources already populated.");
        }

        if (!(await context.ApiScopes.AnyAsync()))
        {
            logger.LogDebug("ApiScopes being populated.");

            foreach (ApiScope scope in ConfigurationData.ApiScopes.ToList())
            {
                context.ApiScopes.Add(scope.ToEntity());
            }

            await context.SaveChangesAsync();
        }
        else
        {
            logger.LogDebug("ApiScopes already populated.");
        }

        //if (!(await context.ApiResources.AnyAsync()))
        //{
        //    logger.LogDebug("ApiResources being populated.");
            
        //    foreach (ApiResource resource in ConfigurationData.ApiResources.ToList())
        //    {
        //        context.ApiResources.Add(resource.ToEntity());
        //    }

        //    await context.SaveChangesAsync();
        //}
        //else
        //{
        //    logger.LogDebug("ApiResources already populated.");
        //}

        if (!(await context.IdentityProviders.AnyAsync()))
        {
            //logger.LogDebug("OIDC IdentityProviders being populated.");
            
            //context.IdentityProviders.Add(new OidcProvider
            //{
            //    Scheme = "demoidsrv",
            //    DisplayName = "IdentityServer",
            //    Authority = "https://demo.duendesoftware.com",
            //    ClientId = "login",
            //}.ToEntity());

            //await context.SaveChangesAsync();
        }
        else
        {
            logger.LogDebug("OIDC IdentityProviders already populated.");
        }

        await context.SaveChangesAsync();
    }
}
