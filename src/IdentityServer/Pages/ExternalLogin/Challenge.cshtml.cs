using Duende.IdentityServer.Services;
using Enterprise.Applications.AspNetCore.Security.ReturnUrls.Extensions;
using IdentityServer.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.ExternalLogin;

[AllowAnonymous]
public class Challenge : PageModel
{
    private readonly IIdentityServerInteractionService _interactionService;
    private readonly ILogger<Challenge> _logger;

    public Challenge(IIdentityServerInteractionService interactionService,
        ILogger<Challenge> logger)
    {
        _interactionService = interactionService;
        _logger = logger;
    }
        
    public IActionResult OnGet(string scheme, string? returnUrl)
    {
        if (string.IsNullOrEmpty(returnUrl))
        {
            returnUrl = PathConstants.RootRelativePath;
        }

        // Validate return URL.
        // Either it is a valid OIDC URL or back to a local page.
        if (!Url.IsLocalUrl(returnUrl) && !_interactionService.IsValidReturnUrl(returnUrl))
        {
            _logger.LogInvalidReturnUrl(returnUrl);
            throw new ArgumentException(ErrorMessages.InvalidReturnUrl);
        }
            
        // Start challenge and roundtrip the return URL and scheme.
        var props = new AuthenticationProperties
        {
            RedirectUri = Url.Page(PageConstants.ExternalLoginCallback),
            Items =
            {
                { ParameterNames.Scheme, scheme },
                { ParameterNames.ReturnUrl, returnUrl }
            }
        };

        return Challenge(props, scheme);
    }
}
