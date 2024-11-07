using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityModel;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Security.Claims;

namespace IdentityServer.Configuration.Ciba;

// https://docs.duendesoftware.com/identityserver/v7/ui/ciba/
// https://docs.duendesoftware.com/identityserver/v7/reference/endpoints/ciba/
// https://www.identityserver.com/articles/ciba-in-identityserver
// https://openid.net/specs/openid-client-initiated-backchannel-authentication-core-1_0.html
// https://openid.net/wg/fapi/

public class BackchannelAuthenticationUserNotificationService : IBackchannelAuthenticationUserNotificationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BackchannelAuthenticationUserNotificationService> _logger;

    public BackchannelAuthenticationUserNotificationService(
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender,
        IConfiguration configuration,
        ILogger<BackchannelAuthenticationUserNotificationService> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendLoginRequestAsync(BackchannelUserLoginRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        string backchannelLoginId = request.InternalId;

        string? userId = request.Subject.FindFirstValue(JwtClaimTypes.Subject);
        string? displayName = request.Subject.FindFirstValue(JwtClaimTypes.Name);
        string? email = request.Subject.FindFirstValue(JwtClaimTypes.Email);

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogError("Subject claim not found in the request.");
            throw new InvalidOperationException(ErrorMessages.UnknownUserId);
        }

        ApplicationUser? user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            _logger.LogError("User not found for ID: {UserId}.", userId);
            throw new InvalidOperationException(ErrorMessages.UserNotFound);
        }

        if (string.IsNullOrEmpty(user.Email))
        {
            _logger.LogError("User {UserId} does not have an email address.", userId);
            return;
        }

        const string subject = "Login Request Notification";
        string name = GetUserNameFromClaims(request.Subject);
        string message = GenerateEmailMessage(request, name, _configuration);

        try
        {
            await _emailSender.SendEmailAsync(user.Email, subject, message);
            _logger.LogInformation("Login request notification email sent to {Email} for request {RequestId}.", user.Email, request.InternalId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending login request notification email to {Email} for request {RequestId}.", user.Email, request.InternalId);
            throw;
        }
    }

    private static string GetUserNameFromClaims(ClaimsPrincipal subject)
    {
        string? name = subject.FindFirstValue(JwtClaimTypes.Name);

        if (!string.IsNullOrEmpty(name))
        {
            return name;
        }

        string? givenName = subject.FindFirstValue(JwtClaimTypes.GivenName);
        string? familyName = subject.FindFirstValue(JwtClaimTypes.FamilyName);

        if (!string.IsNullOrEmpty(givenName) && !string.IsNullOrEmpty(familyName))
        {
            return $"{givenName} {familyName}";
        }

        return "User"; // Fallback to a generic term if no name information is found
    }

    private static string GenerateEmailMessage(BackchannelUserLoginRequest request, string name, IConfiguration configuration)
    {
        // TODO: Use better formatting (easier to maintain)
        // Better yet, externalize this into a template.

        string companyDisplayName = configuration.GetValue<string>(ConfigurationKeys.CompanyDisplayName);

        return $@"Dear {name},

You have received a login request from {request.Client.ClientName}.

Request Details:
- Binding Message: {request.BindingMessage}

Please visit the following link to complete the login process:
{GenerateLoginLink(request)}

If you did not initiate this request, please ignore this email and contact support immediately.

Thank you,
{companyDisplayName}";
    }

    private static string GenerateLoginLink(BackchannelUserLoginRequest request)
    {
        // TODO: Replace with actual login URL.
        return $"https://localhost:5001/ciba?id={request.InternalId}";
    }
}
