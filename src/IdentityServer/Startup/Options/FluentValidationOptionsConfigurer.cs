using Enterprise.Applications.AspNetCore.Startup.Options;
using Enterprise.Applications.AspNetCore.Startup.Options.Abstract;
using Enterprise.Applications.AspNetCore.Startup.Options.Extensions;

namespace IdentityServer.Startup.Options;

public class FluentValidationOptionsConfigurer : IConfigureAppOptions
{
    public static void Configure(AppOptions options, IConfiguration configuration, IHostEnvironment environment)
    {
        options.ConfigureFluentValidation(fluentValidationOptions =>
        {
            fluentValidationOptions.AddAssembly(AssemblyReference.Assembly);
        });
    }
}