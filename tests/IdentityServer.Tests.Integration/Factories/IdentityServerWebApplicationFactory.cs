using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IdentityServer.Tests.Integration.Factories;

public sealed class IdentityServerWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting(
            $"ConnectionStrings:{AspNetIdentity.Constants.ConnectionStringKeys.AspNetIdentity}",
            "Server=(localdb)\\MSSQLLocalDB;Database=AspNetIdentity_Integration;Trusted_Connection=true;MultipleActiveResultSets=true;"
        );

        builder.UseSetting(
            $"ConnectionStrings:{Configuration.Constants.ConnectionStringKeys.IdentityServer}",
            "Server=(localdb)\\MSSQLLocalDB;Database=IdentityServer_Integration;Trusted_Connection=true;MultipleActiveResultSets=true;"
        );

        builder.ConfigureAppConfiguration((context, configurationBuilder) =>
        {
            // This fired way too late...
            // The connection strings are used in the service registration for AspNetIdentity and IdentityServer.
            // This meant the connection strings were being pulled from the appsettings.json.
            //configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            //{
            //    { 
            //        $"ConnectionStrings:{AspNetIdentityConstants.ConnectionStringKeyName}", 
            //        "Server=(localdb)\\MSSQLLocalDB;Database=AspNetIdentity_Integration;Trusted_Connection=true;MultipleActiveResultSets=true;"
            //    },
            //    { 
            //        $"ConnectionStrings:{ConfigurationConstants.ConnectionStringKeyName}", 
            //        "Server=(localdb)\\MSSQLLocalDB;Database=IdentityServer_Integration;Trusted_Connection=true;MultipleActiveResultSets=true;"
            //    }
            //});
        });

        base.ConfigureWebHost(builder);
    }
}
