using Authsignal;
using Enterprise.Applications.AspNetCore.Security.Authentication.Extensions;
using Enterprise.ApplicationServices.Core.Commands.Dispatching;
using Enterprise.ApplicationServices.Core.Queries.Dispatching.Facade;
using Enterprise.Patterns.ResultPattern.Errors.Extensions;
using Enterprise.Patterns.ResultPattern.Errors.Model.Abstract;
using Enterprise.Patterns.ResultPattern.Model;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.Constants;
using IdentityServer.Modules.IdentityManagement.UseCases.Passwords.ChangePassword;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.DoesUserHavePassword;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.GetLoggedInUser;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.RefreshSignIn;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using IdentityServer.Security.Mfa.AuthSignal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Pages.Account.Manage;

public class ChangePasswordModel : PageModel
{
    private readonly IConfiguration _configuration;
    private readonly IAuthsignalClient _authsignalClient;
    private readonly IDispatchCommands _commandDispatcher;
    private readonly IQueryDispatchFacade _queryDispatcher;
    private readonly AuthsignalTrackingService _authsignalTrackingService;
    private readonly ILogger<ChangePasswordModel> _logger;

    public ChangePasswordModel(
        IConfiguration configuration,
        IAuthsignalClient authsignalClient,
        IDispatchCommands commandDispatcher,
        IQueryDispatchFacade queryDispatcher,
        AuthsignalTrackingService authsignalTrackingService,
        ILogger<ChangePasswordModel> logger)
    {
        _configuration = configuration;
        _authsignalClient = authsignalClient;
        _commandDispatcher = commandDispatcher;
        _queryDispatcher = queryDispatcher;
        _authsignalTrackingService = authsignalTrackingService;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    [TempData]
    public string StatusMessage { get; set; }

    public class InputModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(string? token = null)
    {
        var getLoggedInUserQuery = new GetLoggedInUserQuery();
        Result<User> getLoggedInUserResult = await _queryDispatcher.DispatchAsync(getLoggedInUserQuery);

        if (getLoggedInUserResult.Failed)
        {
            if (getLoggedInUserResult.Errors.ContainsNotFound())
            {
                return NotFound("The logged in user could not be obtained.");
            }

            foreach (IError error in getLoggedInUserResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Message);
            }

            return Page();
        }

        User user = getLoggedInUserResult.Value;

        // If Authsignal is enabled, we want to issue a challenge before allowing them to access this page.
        if (_configuration.GetValue(ConfigurationKeys.AuthsignalEnabled, false))
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                string redirectUrl = Url.PageLink(AccountManagementPageConstants.ChangePassword);

                TrackResponse response = await _authsignalTrackingService.GetTrackResponseAsync(
                    "change-password",
                    redirectUrl,
                    user.UserId,
                    user.Username,
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

        var doesUserHavePasswordQuery = new DoesUserHavePasswordQuery(user.UserId);
        bool hasPassword = await _queryDispatcher.DispatchAsync(doesUserHavePasswordQuery);

        if (!hasPassword)
        {
            return RedirectToPage(AccountManagementPageConstants.SetPassword);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        string? userId = User.GetUserIdFromClaims();

        if (string.IsNullOrWhiteSpace(userId))
        {
            ModelState.AddModelError(string.Empty, "Could not obtain user ID from claims.");
            return Page();
        }

        var changePasswordCommand = new ChangePasswordCommand(userId, Input.OldPassword, Input.NewPassword);
        Result changePasswordResult = await _commandDispatcher.DispatchAsync(changePasswordCommand);

        if (changePasswordResult.Failed)
        {
            if (changePasswordResult.Errors.ContainsNotFound())
            {
                return NotFound($"Unable to load user with ID: '{userId}'.");
            }

            foreach (IError error in changePasswordResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Message);
            }

            return Page();
        }

        StatusMessage = "Your password has been changed.";

        var refreshSignInCommand = new RefreshSignInCommand();
        Result refreshSignInResult = await _commandDispatcher.DispatchAsync(refreshSignInCommand);

        return RedirectToPage();
    }
}