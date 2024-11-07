using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityServer.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace IdentityServer.Pages.Error;

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling?view=aspnetcore-8.0#exception-handler-page
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling?view=aspnetcore-8.0#access-the-exception

[AllowAnonymous]
[IgnoreAntiforgeryToken]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class IndexModel : PageModel
{
    private readonly IConfiguration _configuration;
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<IndexModel> _logger;

    public ErrorMessage? Error { get; set; }

    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    public Activity? CurrentActivity { get; set; }
    public string? TraceId { get; set; }
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }

    public IndexModel(
        IConfiguration configuration,
        IIdentityServerInteractionService interaction,
        IWebHostEnvironment environment,
        ILogger<IndexModel> logger)
    {
        _configuration = configuration;
        _interaction = interaction;
        _environment = environment;
        _logger = logger;
    }

    public async Task OnGet(string? errorId = null, string? errorMessage = null, string? statusCode = null)
    {
        bool showErrors = _configuration.GetValue(ConfigurationKeys.ShowErrors, false);

        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        CurrentActivity = Activity.Current;
        TraceId = HttpContext.TraceIdentifier;

        string? userName = User.Identity?.IsAuthenticated ?? false ? User.Identity.Name : string.Empty;

        // Retrieve error details.
        ErrorMessage? message = await _interaction.GetErrorContextAsync(errorId);

        if (message != null)
        {
            Error = message;
            RequestId = Error.RequestId;

            if (!_environment.IsDevelopment() && !showErrors)
            {
                // Only show in development (unless configured).
                // TODO: Log error?
                message.ErrorDescription = null;
            }
        }
        else
        {
            ErrorMessage = errorMessage;

            IExceptionHandlerPathFeature? exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionHandlerPathFeature != null )
            {
                Exception exception = exceptionHandlerPathFeature.Error;
                string path = exceptionHandlerPathFeature.Path;
                Endpoint? endpoint = exceptionHandlerPathFeature.Endpoint;
                RouteValueDictionary? routeValues = exceptionHandlerPathFeature.RouteValues;

                if (_environment.IsDevelopment() || showErrors)
                {
                    Exception = exception;
                }
            }
        }
    }
}
