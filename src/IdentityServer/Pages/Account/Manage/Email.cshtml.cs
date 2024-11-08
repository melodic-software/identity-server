using Authsignal;
using IdentityServer.AspNetIdentity.Email;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Constants;
using IdentityServer.Security.Mfa.AuthSignal;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

namespace IdentityServer.Pages.Account.Manage;

public class EmailModel : PageModel
{
    private readonly IConfiguration _configuration;
    private readonly IAuthsignalClient _authsignalClient;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly EmailService _emailService;
    private readonly AuthsignalTrackingService _authsignalTrackingService;

    public EmailModel(
        IConfiguration configuration,
        IAuthsignalClient authsignalClient,
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender,
        EmailService emailService,
        AuthsignalTrackingService authsignalTrackingService)
    {
        _configuration = configuration;
        _authsignalClient = authsignalClient;
        _userManager = userManager;
        _emailSender = emailSender;
        _emailService = emailService;
        _authsignalTrackingService = authsignalTrackingService;
    }

    public string? Email { get; set; }
    public bool IsEmailConfirmed { get; set; }

    [TempData]
    public string StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "New email")]
        public string? NewEmail { get; set; }
    }

    private async Task LoadAsync(ApplicationUser user)
    {
        string? email = await _userManager.GetEmailAsync(user);

        Email = email;

        Input = new InputModel
        {
            NewEmail = email,
        };

        IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
    }

    public async Task<IActionResult> OnGetAsync(string? token = null)
    {
        ApplicationUser user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        // If Authsignal is enabled, we want to issue a challenge before allowing them to access this page.
        if (_configuration.GetValue(ConfigurationKeys.AuthsignalEnabled, false))
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                string redirectUrl = Url.PageLink(AccountManagementPageConstants.ManageEmail);

                TrackResponse response = await _authsignalTrackingService.GetTrackResponseAsync(
                    "manage-email",
                    redirectUrl,
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.PhoneNumber,
                    deviceId: null,
                    null,
                    CancellationToken.None
                );

                if (response.State == UserActionState.CHALLENGE_REQUIRED)
                {
                    return Redirect(response.Url);
                }
            }
            else
            {
                var validateChallengeRequest = new ValidateChallengeRequest(Token: token);

                ValidateChallengeResponse validateChallengeResponse = await _authsignalClient.ValidateChallenge(validateChallengeRequest);

                if (validateChallengeResponse.State != UserActionState.CHALLENGE_SUCCEEDED)
                {
                    return RedirectToPage(PageConstants.AccessDenied);
                }
            }
        }

        await LoadAsync(user);

        return Page();
    }

    public async Task<IActionResult> OnPostChangeEmailAsync()
    {
        ApplicationUser user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }

        string email = await _userManager.GetEmailAsync(user);

        if (Input.NewEmail != email)
        {
            if (string.IsNullOrWhiteSpace(Input.NewEmail))
            {
                throw new InvalidOperationException("New email must be provided.");
            }

            string userId = await _userManager.GetUserIdAsync(user);

            string encodedToken = await _emailService.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
            Uri callbackUrl = _emailService.GenerateEmailChangeConfirmationLink(Url, HttpContext, userId, encodedToken, Input.NewEmail);

            // TODO: Complete converting this, absorb as much into the email sender as the others.
            // This will likely require a new template. Methods can be made private afterward.
            // This should also be converted into a command/command handler since it represents a use case.

            await _emailSender.SendEmailAsync(
                Input.NewEmail,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl.ToString())}'>clicking here</a>.");

            StatusMessage = "Confirmation link to change email sent. Please check your email.";

            return RedirectToPage();
        }

        StatusMessage = "Your email is unchanged.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSendVerificationEmailAsync()
    {
        ApplicationUser user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }

        string userId = await _userManager.GetUserIdAsync(user);

        string email = await _userManager.GetEmailAsync(user);

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("New email must be provided.");
        }

        string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        string callbackUrl = Url.Page(
            AccountPageConstants.ConfirmEmail,
            pageHandler: null,
            values: new { userId, code },
            protocol: Request.Scheme);

        

        if (string.IsNullOrWhiteSpace(callbackUrl))
        {
            throw new InvalidOperationException("Callback URL cannot be null.");
        }

        await _emailSender.SendEmailAsync(
            email,
            "Confirm your email",
            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

        StatusMessage = "Verification email sent. Please check your email.";

        return RedirectToPage();
    }
}