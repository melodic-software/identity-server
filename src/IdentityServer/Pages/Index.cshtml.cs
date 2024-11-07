using Duende.IdentityServer;
using Duende.IdentityServer.Hosting;
using Enterprise.Library.Core.Extensions;
using IdentityServer.Security.Authentication.SignOut.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;

namespace IdentityServer.Pages;

[AllowAnonymous]
public class Index : PageModel
{
    private readonly IHostEnvironment _environment;
    private readonly SwaggerUIOptions _swaggerOptions;

    public Index(IHostEnvironment environment, IOptions<SwaggerUIOptions> swaggerOptions, IdentityServerLicense? license = null)
    {
        _environment = environment;
        _swaggerOptions = swaggerOptions.Value;

        License = license;
    }

    public string DiscoveryDocumentPath { get; set; } = "/.well-known/openid-configuration";
    public bool ShowSwaggerLink { get; set; }
    public string SwaggerPath { get; set; } = "/swagger";
    public bool ShowDiagnosticsLink { get; set; }

    public string Version => typeof(IdentityServerMiddleware).Assembly
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
        ?.InformationalVersion.Split('+').First() ?? "unavailable";

    public IdentityServerLicense? License { get; }

    public async Task<IActionResult> OnGet(string? returnUrl = null)
    {
        // Ensure the temporary cookie used during external authentication is removed.
        // TODO: Is there a way to check if the user is signed in with the external scheme?
        await HttpContext.SignOutExternalAsync();

        ShowSwaggerLink = !_environment.IsProduction();
        ShowDiagnosticsLink = _environment.IsDevelopment();

        SwaggerPath = _swaggerOptions.RoutePrefix.EnsureLeadingForwardSlash();

        return Page();
    }
}
