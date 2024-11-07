using Enterprise.Applications.AspNetCore.Startup.Options;
using Enterprise.Applications.AspNetCore.Startup.Options.Abstract;
using Enterprise.Applications.AspNetCore.Startup.Options.Extensions;

namespace IdentityServer.Startup.Options;

internal sealed class MediatROptionsConfigurer : IConfigureAppOptions
{
    public static void Configure(AppOptions options, IConfiguration configuration, IHostEnvironment environment)
    {
        options.ConfigureMediatR(mediatROptions =>
        {
            mediatROptions.AddAssembly(AssemblyReference.Assembly);
        });
    }
}
