using Duende.IdentityServer.Validation;
using IdentityServer.AspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IdentityServer.Configuration.Ciba;

// https://docs.duendesoftware.com/identityserver/v7/ui/ciba/
// https://docs.duendesoftware.com/identityserver/v7/reference/endpoints/ciba/
// https://www.identityserver.com/articles/ciba-in-identityserver
// https://openid.net/specs/openid-client-initiated-backchannel-authentication-core-1_0.html
// https://openid.net/wg/fapi/

public class BackChannelAuthenticationUserValidator : IBackchannelAuthenticationUserValidator
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<BackChannelAuthenticationUserValidator> _logger;

    public BackChannelAuthenticationUserValidator(
        UserManager<ApplicationUser> userManager,
        ILogger<BackChannelAuthenticationUserValidator> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<BackchannelAuthenticationUserValidationResult> ValidateRequestAsync(
        BackchannelAuthenticationUserValidatorContext userValidatorContext)
    {
        var result = new BackchannelAuthenticationUserValidationResult();

        // Check if the login hint is provided.
        if (string.IsNullOrEmpty(userValidatorContext.LoginHint))
        {
            _logger.LogWarning("Login hint is required.");
            result.Error = "invalid_request";
            result.ErrorDescription = "Login hint is required.";
            return result;
        }

        // Find the user by the login hint (assuming it's an email or username).
        ApplicationUser? user = await _userManager.FindByNameAsync(userValidatorContext.LoginHint)
                                ?? await _userManager.FindByEmailAsync(userValidatorContext.LoginHint);

        if (user == null)
        {
            _logger.LogWarning("User not found for login hint: {LoginHint}.", userValidatorContext.LoginHint);
            result.Error = "invalid_user";
            result.ErrorDescription = "User not found";
            return result;
        }

        if (!string.IsNullOrEmpty(userValidatorContext.BindingMessage))
        {
            bool isValidBindingMessage = ValidateBindingMessage(userValidatorContext.BindingMessage);

            if (!isValidBindingMessage)
            {
                _logger.LogWarning("Invalid binding message: {BindingMessage}.", userValidatorContext.BindingMessage);
                result.Error = "invalid_binding_message";
                result.ErrorDescription = "The provided binding message is invalid.";
                return result;
            }
        }

        // Custom validation: Check if the user account is enabled
        //if (!user.IsEnabled)
        //{
        //    _logger.LogWarning("User account is disabled: {UserId}.", user.Id);
        //    result.Error = "invalid_request";
        //    result.ErrorDescription = "User account is disabled";
        //    return result;
        //}

        IList<Claim> claims = await _userManager.GetClaimsAsync(user);

        result.Subject = new ClaimsPrincipal(new ClaimsIdentity(claims));

        return result;
    }

    private static bool ValidateBindingMessage(string bindingMessage)
    {
        // TODO: Implement binding message validation logic here.
        // This is just a placeholder implementation.
        return !string.IsNullOrEmpty(bindingMessage);
    }
}
