using Enterprise.Applications.AspNetCore.Startup.Options;
using Enterprise.Applications.AspNetCore.Startup.Options.Abstract;
using Enterprise.Applications.AspNetCore.Startup.Options.Extensions;

namespace IdentityServer.Startup.Options;

internal sealed class MinimalApiOptionsConfigurer : IConfigureAppOptions
{
    public static void Configure(AppOptions options, IConfiguration configuration, IHostEnvironment environment)
    {
        options.ConfigureMinimalApi(minimalApiOptions =>
        {
            minimalApiOptions.AddEndpointAssembly(AssemblyReference.Assembly);
        });
    }
}
