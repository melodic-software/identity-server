using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Constants;
using IdentityServer.Logging;
using IdentityServer.Security.Authentication.Model;
using IdentityServer.Security.Authentication.Results;
using IdentityServer.Security.Authentication.Schemes.Abstract;
using IdentityServer.Security.Authentication.SignOut.Extensions;
using IdentityServer.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace IdentityServer.Pages.Account.Manage;

public class ExternalLoginsModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IAuthenticationSchemeService _authenticationSchemeService;
    private readonly IUserStore<ApplicationUser> _userStore;
    private readonly IAuthenticationSchemeProvider _schemeProvider;
    private readonly ILogger<ExternalLoginsModel> _logger;

    public const string ErrorPrefix = "Error";

    public ExternalLoginsModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IAuthenticationSchemeService authenticationSchemeService,
        IUserStore<ApplicationUser> userStore,
        IAuthenticationSchemeProvider schemeProvider,
        ILogger<ExternalLoginsModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _authenticationSchemeService = authenticationSchemeService;
        _userStore = userStore;
        _schemeProvider = schemeProvider;
        _logger = logger;
    }

    public IList<UserLoginInfo> CurrentLogins { get; set; }
    public IList<ExternalProvider> OtherLogins { get; set; }
    public bool ShowRemoveButton { get; set; }
    
    [TempData]
    public string StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        ApplicationUser user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        CurrentLogins = await _userManager.GetLoginsAsync(user);

        OtherLogins = (await _authenticationSchemeService.GetProvidersAsync())
            .Where(auth => CurrentLogins.All(ul => auth.AuthenticationScheme != ul.LoginProvider))
            .ToList();

        string passwordHash = null;

        if (_userStore is IUserPasswordStore<ApplicationUser> userPasswordStore)
        {
            passwordHash = await userPasswordStore.GetPasswordHashAsync(user, HttpContext.RequestAborted);
        }

        ShowRemoveButton = passwordHash != null || CurrentLogins.Count > 1;

        return Page();
    }

    public async Task<IActionResult> OnPostRemoveLoginAsync(string loginProvider, string providerKey)
    {
        ApplicationUser user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        IdentityResult result = await _userManager.RemoveLoginAsync(user, loginProvider, providerKey);

        if (!result.Succeeded)
        {
            StatusMessage = $"{ErrorPrefix}: The external login was not removed.";
            return RedirectToPage();
        }

        await _signInManager.RefreshSignInAsync(user);
        StatusMessage = "The external login was removed.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostLinkLoginAsync(string scheme)
    {
        // Clear the existing external cookie to ensure a clean login process.
        await HttpContext.SignOutExternalAsync();

        // Request a redirect to the external login provider to link a login for the current user.
        string returnUrl = Url.Page(AccountManagementPageConstants.ManageExternalLogins, pageHandler: PageHandlers.LinkLoginCallback);

        // Start challenge and roundtrip the return URL and scheme.
        var props = new AuthenticationProperties
        {
            RedirectUri = returnUrl,
            Items =
            {
                { ParameterNames.Scheme, scheme },
                { ParameterNames.ReturnUrl, returnUrl }
            }
        };

        return Challenge(props, scheme);
    }

    public async Task<IActionResult> OnGetLinkLoginCallbackAsync()
    {
        ApplicationUser authenticatedUser = await _userManager.GetUserAsync(User);

        if (authenticatedUser == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        // Read external identity from the temporary cookie.
        AuthenticateResult authenticateResult = await HttpContext.AuthenticateExternalAsync();

        if (!authenticateResult.Succeeded)
        {
            throw new InvalidOperationException(ErrorMessages.ExternalAuthenticationError(authenticateResult));
        }

        ClaimsPrincipal externalUser = authenticateResult.GetPrincipal();

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            IEnumerable<string> externalClaims = externalUser.Claims.Select(c => $"{c.Type}: {c.Value}");
            _logger.ExternalClaims(externalClaims);
        }

        string externalProvider = authenticateResult.GetExternalProvider();

        AuthenticationScheme? authenticationScheme = await _schemeProvider.GetSchemeAsync(externalProvider);

        if (authenticationScheme == null)
        {
            throw new InvalidOperationException(ErrorMessages.AuthenticationSchemeNotFound);
        }

        string externalUserId = externalUser.GetExternalUserId();

        ApplicationUser? userWithLogin = await _userManager.FindByLoginAsync(externalProvider, externalUserId);

        if (userWithLogin != null)
        {
            StatusMessage = $"{ErrorPrefix}: External logins can only be associated with one account.";
            return RedirectToPage();
        }

        var info = new UserLoginInfo(authenticationScheme.Name, externalUserId, authenticationScheme.DisplayName);

        IdentityResult addLoginResult = await _userManager.AddLoginAsync(authenticatedUser, info);

        if (!addLoginResult.Succeeded)
        {
            StatusMessage = $"{ErrorPrefix}: ";
            StatusMessage += addLoginResult.Errors.FirstOrDefault()?.Description ?? "The external login could not be added.";
            return RedirectToPage();
        }

        // Clear the existing external cookie.
        await HttpContext.SignOutExternalAsync();

        StatusMessage = "The external login was added.";

        return RedirectToPage();
    }
}