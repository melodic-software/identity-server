using Enterprise.Applications.AspNetCore.Startup.Options;
using Enterprise.Applications.AspNetCore.Startup.Options.Abstract;
using Enterprise.Applications.AspNetCore.Startup.Options.Extensions;
using IdentityServer.Pages;

namespace IdentityServer.Startup.Options;

internal sealed class ErrorHandlingOptionsConfigurer : IConfigureAppOptions
{
    public static void Configure(AppOptions options, IConfiguration configuration, IHostEnvironment environment)
    {
        options.ConfigureErrorHandling(errorHandlingOptions =>
        {
            errorHandlingOptions.ExceptionHandlerOptions = new ExceptionHandlerOptions
            {
                ExceptionHandlingPath = PageConstants.Error
            };

            errorHandlingOptions.AddDevelopmentEnvironmentMiddleware += app =>
            {
                // TODO: What is this?
                app.UseMigrationsEndPoint();
            };
        });
    }
}
