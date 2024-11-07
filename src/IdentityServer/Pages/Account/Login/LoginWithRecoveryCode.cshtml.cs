using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Security.Authentication.SignIn;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace IdentityServer.Pages.Account.Login;

[AllowAnonymous]
public class LoginWithRecoveryCodeModel : PageModel
{
    private readonly IIdentityServerInteractionService _interactionService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEventService _events;
    private readonly ILogger<LoginWithRecoveryCodeModel> _logger;

    public LoginWithRecoveryCodeModel(
        IIdentityServerInteractionService interactionService,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IEventService events,
        ILogger<LoginWithRecoveryCodeModel> logger)
    {
        _interactionService = interactionService;
        _signInManager = signInManager;
        _userManager = userManager;
        _events = events;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; }
    public string ReturnUrl { get; set; }

    public class InputModel
    {
        [BindProperty]
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Recovery Code")]
        public string RecoveryCode { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
    {
        // Ensure the user has gone through the username & password screen first
        ApplicationUser user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

        if (user == null)
        {
            throw new InvalidOperationException($"Unable to load two-factor authentication user.");
        }

        returnUrl ??= Url.Content(PathConstants.RootRelativePath);

        ReturnUrl = returnUrl;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        returnUrl ??= Url.Content(PathConstants.RootRelativePath);

        ApplicationUser user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

        if (user == null)
        {
            throw new InvalidOperationException("Unable to load two-factor authentication user.");
        }

        string recoveryCode = Input.RecoveryCode.Replace(" ", string.Empty);

        SignInResult result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

        string userId = await _userManager.GetUserIdAsync(user);

        if (result.Succeeded)
        {
            _logger.LogInformation("User with ID '{UserId}' logged in with a recovery code.", user.Id);

            // Check if we are in the context of an authorization request.
            AuthorizationRequest authorizationRequest = await _interactionService.GetAuthorizationContextAsync(returnUrl);
            await SignInSuccessService.HandleSuccessfulSignInAsync(user, _userManager, authorizationRequest, _events);

            return LocalRedirect(returnUrl);
        }
        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account locked out.");
            return RedirectToPage(AccountPageConstants.Lockout);
        }

        _logger.LogWarning("Invalid recovery code entered for user with ID '{UserId}' ", user.Id);
        ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
        return Page();
    }
}