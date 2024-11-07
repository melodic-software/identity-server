using Enterprise.Applications.AspNetCore.Startup.Options;
using Enterprise.Applications.AspNetCore.Startup.Options.Abstract;
using Enterprise.Applications.AspNetCore.Startup.Options.Extensions;

namespace IdentityServer.Diagnostics;

internal sealed class OpenTelemetryOptionsConfigurer : IConfigureAppOptions
{
    public static void Configure(AppOptions options, IConfiguration configuration, IHostEnvironment environment)
    {
        options.ConfigureOpenTelemetry(o =>
        {
            //o.ServiceAttributes.Add(new("key", "value"));

            o.ActivitySourceNames.Add(ApplicationDiagnostics.ActivitySourceName);

            o.MeterNames.Add(Telemetry.ServiceName);
            o.MeterNames.Add(ApplicationDiagnostics.Meter.Name);
        });
    }
}