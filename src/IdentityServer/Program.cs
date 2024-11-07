using Enterprise.Applications.AspNetCore.Startup;

// https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview?view=aspnetcore-8.0
// https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/advanced?view=aspnetcore-8.0#data-protection

await Application.RunAsync(args, ApplicationType.RazorPages);

public partial class Program;
