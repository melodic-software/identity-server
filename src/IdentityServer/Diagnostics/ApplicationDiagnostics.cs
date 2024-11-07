using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace IdentityServer.Diagnostics;

// https://courses.dometrain.com/courses/take/from-zero-to-hero-open-telemetry-in-net/lessons/56512314-manually-creating-spans

public static class ApplicationDiagnostics
{
    public const string ServiceName = "IdentityServer";
    public const string ActivitySourceName = "IdentityServer";

    public static readonly Meter Meter = new(ServiceName);
    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    // TODO: Add custom metrics / instrumentation types here.
    // There are some already covered in Telemetry.cs (IdentityServer provided).

    //public static Counter<long> AttemptedLoginCounter { get; } = Meter.CreateCounter<long>("logins.attempted");
    //public static Counter<long> FailedLoginCounter { get; } = Meter.CreateCounter<long>("logins.failed");
    //public static Counter<long> SuccessfulLoginCounter { get; } = Meter.CreateCounter<long>("logins.successful");
}
