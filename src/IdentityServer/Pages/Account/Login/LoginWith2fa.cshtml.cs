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
public class LoginWith2FaModel : PageModel
{
    private readonly IIdentityServerInteractionService _interaction;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEventService _events;
    private readonly ILogger<LoginWith2FaModel> _logger;

    public LoginWith2FaModel(
        IIdentityServerInteractionService interaction,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IEventService events,
        ILogger<LoginWith2FaModel> logger)
    {
        _interaction = interaction;
        _signInManager = signInManager;
        _userManager = userManager;
        _events = events;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public bool RememberMe { get; set; }
    public string ReturnUrl { get; set; }
    
    public class InputModel
    {
        [Required]
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Authenticator code")]
        public string TwoFactorCode { get; set; }

        [Display(Name = "Remember this machine")]
        public bool RememberMachine { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(bool rememberMe, string? returnUrl = null)
    {
        // Ensure the user has gone through the username & password screen first.
        ApplicationUser user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

        if (user == null)
        {
            throw new InvalidOperationException("Unable to load two-factor authentication user.");
        }

        returnUrl ??= Url.Content(PathConstants.RootRelativePath);

        ReturnUrl = returnUrl;
        RememberMe = rememberMe;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(bool rememberMe, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        returnUrl ??= Url.Content(PathConstants.RootRelativePath);

        ApplicationUser user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        
        if (user == null)
        {
            throw new InvalidOperationException($"Unable to load two-factor authentication user.");
        }

        string authenticatorCode = Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

        SignInResult result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, Input.RememberMachine);

        string userId = await _userManager.GetUserIdAsync(user);

        if (result.Succeeded)
        {
            _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.Id);

            // Check if we are in the context of an authorization request.
            AuthorizationRequest authorizationRequest = await _interaction.GetAuthorizationContextAsync(returnUrl);
            await SignInSuccessService.HandleSuccessfulSignInAsync(user, _userManager, authorizationRequest, _events);

            return LocalRedirect(returnUrl);
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
            return RedirectToPage(AccountPageConstants.Lockout);
        }

        _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);
        ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
        return Page();
    }
}