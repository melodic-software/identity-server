using Enterprise.Applications.AspNetCore.Startup.Options;
using Enterprise.Applications.AspNetCore.Startup.Options.Abstract;
using Enterprise.Applications.AspNetCore.Startup.Options.Extensions;

namespace IdentityServer.Startup.Options;

internal sealed class SwaggerOptionsConfigurer : IConfigureAppOptions
{
    public static void Configure(AppOptions options, IConfiguration configuration, IHostEnvironment environment)
    {
        options.ConfigureSwaggerUI(uiOptions =>
        {
            uiOptions.PostConfigureUI = o =>
            {
                o.InjectStylesheet("/swagger-ui/custom.css");
                o.InjectJavascript("/swagger-ui/custom.js");
            };
        });
    }
}
