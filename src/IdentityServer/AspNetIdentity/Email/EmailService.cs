using Duende.IdentityServer.Models;
using Humanizer;
using Humanizer.Localisation;
using IdentityServer.AspNetIdentity.Email.Templates.Constants;
using IdentityServer.AspNetIdentity.Email.Templates.Services;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Constants;
using IdentityServer.Pages;
using IdentityServer.Pages.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;

// TODO: Split this into a service per concern (password reset, email confirmation, change email confirmation, etc.)

namespace IdentityServer.AspNetIdentity.Email;

public class EmailService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly EmailTemplateService _emailTemplateService;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;
    private readonly DataProtectionTokenProviderOptions _dataProtectionTokenProviderOptions;
    private readonly ILogger<EmailService> _logger;

    public EmailService(UserManager<ApplicationUser> userManager,
        EmailTemplateService emailTemplateService,
        IEmailSender emailSender,
        IConfiguration configuration,
        IOptions<DataProtectionTokenProviderOptions> dataProtectionTokenProviderOptions,
        ILogger<EmailService> logger)
    {
        _userManager = userManager;
        _emailTemplateService = emailTemplateService;
        _emailSender = emailSender;
        _configuration = configuration;
        _dataProtectionTokenProviderOptions = dataProtectionTokenProviderOptions.Value;
        _logger = logger;
    }

    public async Task SendCibaLoginRequestEmailAsync(IUrlHelper urlHelper, HttpContext httpContext, string email, BackchannelUserLoginRequest request)
    {
        Uri loginUrl = GenerateCibaLoginLink(urlHelper, httpContext, request.InternalId);

        string subject = "Login Request Notification";

        string htmlTemplate = _emailTemplateService.LoadCibaLoginRequestTemplate();

        string href = HtmlEncoder.Default.Encode(loginUrl.ToString());

        string htmlMessage = htmlTemplate
            .Replace(TemplateTokenNames.LoginLink, href)
            .Replace(TemplateTokenNames.ClientName, request.Client.ClientName ?? "a client application.")
            .Replace(TemplateTokenNames.BindingMessage, request.BindingMessage ?? "N/A")
            .Replace(TemplateTokenNames.CurrentYear, DateTime.UtcNow.Year.ToString(CultureInfo.InvariantCulture))
            .Replace(TemplateTokenNames.SupportEmailAddress, _configuration.GetValue<string>(ConfigurationKeys.SupportEmailAddress))
            .Replace(TemplateTokenNames.CompanyDisplayName, _configuration.GetValue<string>(ConfigurationKeys.CompanyDisplayName));

        await _emailSender.SendEmailAsync(email, subject, htmlMessage);

        _logger.LogInformation("Login request notification email sent to {Email} for request {RequestId}.", email, request.InternalId);
    }

    public async Task<string> SendConfirmationEmailAsync(IUrlHelper urlHelper, HttpContext httpContext,
        ApplicationUser user, string email, string? returnUrl = null)
    {
        string encodedToken = await GenerateEmailConfirmationTokenAsync(user);

        // This is essentially a callback URL.
        Uri confirmationUrl = GenerateEmailConfirmationLink(urlHelper, httpContext, user.Id, encodedToken, returnUrl);

        _logger.LogInformation("Confirmation link for {Email}: {ConfirmationUrl}", user.Email, confirmationUrl);

        string subject = "Confirm Your Email";

        string htmlTemplate = _emailTemplateService.LoadEmailConfirmationTemplate();

        string href = HtmlEncoder.Default.Encode(confirmationUrl.ToString());
        string humanizedTokenLifespan = GetHumanizedTokenLifespan();

        string htmlMessage = htmlTemplate
            .Replace(TemplateTokenNames.EmailConfirmationLink, href)
            .Replace(TemplateTokenNames.CurrentYear, DateTime.UtcNow.Year.ToString(CultureInfo.InvariantCulture))
            .Replace(TemplateTokenNames.SupportEmailAddress, _configuration.GetValue<string>(ConfigurationKeys.SupportEmailAddress))
            .Replace(TemplateTokenNames.CompanyDisplayName, _configuration.GetValue<string>(ConfigurationKeys.CompanyDisplayName))
            .Replace(TemplateTokenNames.LinkLifespan, humanizedTokenLifespan);

        await _emailSender.SendEmailAsync(email, subject, htmlMessage);

        return encodedToken;
    }

    public async Task<string> SendPasswordResetEmailAsync(IUrlHelper urlHelper, HttpContext httpContext,
        ApplicationUser user)
    {
        string passwordResetToken = await GeneratePasswordResetTokenAsync(user);
        Uri callbackUrl = GenerateResetPasswordLink(urlHelper, httpContext, passwordResetToken);

        _logger.LogInformation("Reset password link for {Email}: {ResetPasswordUrl}", user.Email, callbackUrl);

        string subject = "Reset Password";

        string htmlTemplate = _emailTemplateService.LoadPasswordResetTemplate();

        string href = HtmlEncoder.Default.Encode(callbackUrl.ToString());
        string humanizedTokenLifespan = GetHumanizedTokenLifespan();

        string htmlMessage = htmlTemplate
            .Replace(TemplateTokenNames.PasswordResetLink, href)
            .Replace(TemplateTokenNames.CurrentYear, DateTime.UtcNow.Year.ToString(CultureInfo.InvariantCulture))
            .Replace(TemplateTokenNames.SupportEmailAddress, _configuration.GetValue<string>(ConfigurationKeys.SupportEmailAddress))
            .Replace(TemplateTokenNames.CompanyDisplayName, _configuration.GetValue<string>(ConfigurationKeys.CompanyDisplayName))
            .Replace(TemplateTokenNames.LinkLifespan, humanizedTokenLifespan);

        if (string.IsNullOrWhiteSpace(user.Email))
        {
            throw new InvalidOperationException(
                "The user must have an email address in order to reset their password.");
        }

        await _emailSender.SendEmailAsync(user.Email, subject, htmlMessage);

        return passwordResetToken;
    }

    private async Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user)
    {
        // These tokens may be stored differently depending on the provider.
        // The default for ASP.NET Identity uses the security stamp and doesn't require persisting an actual token.
        string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        return GetEncodedToken(token);
    }

    public async Task<string> GenerateChangeEmailTokenAsync(ApplicationUser user, string newEmail)
    {
        // These tokens may be stored differently depending on the provider.
        // The default for ASP.NET Identity uses the security stamp and doesn't require persisting an actual token.
        string token = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
        return GetEncodedToken(token);
    }

    private static string GetEncodedToken(string token)
    {
        byte[] tokenBytes = Encoding.UTF8.GetBytes(token);
        string encodedToken = WebEncoders.Base64UrlEncode(tokenBytes);
        return encodedToken;
    }

    private async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
    {
        // For more information on how to enable account confirmation
        // and password reset please visit https://go.microsoft.com/fwlink/?LinkID=532713
        string code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        return code;
    }

    private static Uri GenerateCibaLoginLink(IUrlHelper urlHelper, HttpContext httpContext, string internalId)
    {
        string? pageLink = urlHelper.PageLink(
            PageConstants.Ciba,
            pageHandler: null,
            values: new { id = internalId },
            protocol: httpContext.Request.Scheme
        );

        return PageLinkUri(pageLink);
    }

    private static Uri GenerateEmailConfirmationLink(IUrlHelper urlHelper, HttpContext httpContext, string userId,
        string encodedToken, string? returnUrl = null)
    {
        string? pageLink = urlHelper.PageLink(
            AccountPageConstants.ConfirmEmail,
            pageHandler: null,
            values: new { userId, token = encodedToken, returnUrl },
            protocol: httpContext.Request.Scheme
        );

        return PageLinkUri(pageLink);
    }

    public Uri GenerateEmailChangeConfirmationLink(IUrlHelper urlHelper, HttpContext httpContext, string userId,
        string encodedToken, string newEmail)
    {
        string? pageLink = urlHelper.PageLink(
            AccountPageConstants.ConfirmEmailChange,
            pageHandler: null,
            values: new { userId, email = newEmail, code = encodedToken },
            protocol: httpContext.Request.Scheme
        );

        return PageLinkUri(pageLink);
    }

    private static Uri GenerateResetPasswordLink(IUrlHelper urlHelper, HttpContext httpContext, string token)
    {
        string? pageLink = urlHelper.PageLink(
            AccountPageConstants.ResetPassword,
            pageHandler: null,
            values: new
            {
                code = token
            },
            protocol: httpContext.Request.Scheme
        );

        return PageLinkUri(pageLink);
    }

    private static Uri PageLinkUri(string? pageLink)
    {
        if (string.IsNullOrWhiteSpace(pageLink))
        {
            throw new InvalidOperationException("Page link could not be constructed.");
        }

        return new Uri(pageLink);
    }

    private string GetHumanizedTokenLifespan()
    {
        TimeSpan tokenLifespan = _dataProtectionTokenProviderOptions.TokenLifespan;

        // If the tokenLifespan is an exact multiple of days, convert to hours.
        if (tokenLifespan.TotalHours is 24 or 48 or 72)
        {
            return $"{tokenLifespan.TotalHours} hours";
        }

        string humanizedTokenLifespan = tokenLifespan.Humanize(
            minUnit: TimeUnit.Second,
            collectionSeparator: null // Use the current culture's collection formatter to combine the time units.
        );

        return humanizedTokenLifespan;
    }
}
