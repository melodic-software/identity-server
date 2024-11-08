using Authsignal;
using IdentityServer.Constants;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using IdentityServer.Pages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Security.Mfa.AuthSignal;

public class AuthsignalActionResultService
{
    private readonly IConfiguration _configuration;
    private readonly AuthsignalTrackingService _authsignalTrackingService;
    private readonly IAuthsignalClient _authsignalClient;

    public AuthsignalActionResultService(
        IConfiguration configuration,
        AuthsignalTrackingService authsignalTrackingService,
        IAuthsignalClient authsignalClient)
    {
        _configuration = configuration;
        _authsignalTrackingService = authsignalTrackingService;
        _authsignalClient = authsignalClient;
    }

    public async Task<IActionResult?> HandlePageMfa(PageModel page, User user, string actionName, string? redirectUrl, string? token, Func<string, IActionResult> redirectAction)
    {
        // If Authsignal is enabled, we want to issue a challenge before allowing them to access this page.
        if (!_configuration.GetValue(ConfigurationKeys.AuthsignalEnabled, false))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            TrackResponse response = await _authsignalTrackingService.GetTrackResponseAsync(
                actionName,
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
                return redirectAction(response.Url);
            }
        }
        else
        {
            var validateChallengeRequest = new ValidateChallengeRequest(Token: token);

            ValidateChallengeResponse validateChallengeResponse = await _authsignalClient.ValidateChallenge(validateChallengeRequest);

            if (validateChallengeResponse.State != UserActionState.CHALLENGE_SUCCEEDED)
            {
                return page.RedirectToPage(PageConstants.AccessDenied);
            }
        }

        return null;
    }
}