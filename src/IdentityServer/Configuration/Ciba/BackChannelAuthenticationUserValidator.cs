using Duende.IdentityServer.Validation;
using Enterprise.ApplicationServices.Core.Queries.Dispatching.Facade;
using Enterprise.Patterns.ResultPattern.Errors.Extensions;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.Modules.IdentityManagement.UseCases.UserClaims;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.GetUserByEmail;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using System.Security.Claims;

namespace IdentityServer.Configuration.Ciba;

// https://docs.duendesoftware.com/identityserver/v7/ui/ciba/
// https://docs.duendesoftware.com/identityserver/v7/reference/endpoints/ciba/
// https://www.identityserver.com/articles/ciba-in-identityserver
// https://openid.net/specs/openid-client-initiated-backchannel-authentication-core-1_0.html
// https://openid.net/wg/fapi/

public class BackChannelAuthenticationUserValidator : IBackchannelAuthenticationUserValidator
{
    private readonly IQueryDispatchFacade _queryDispatcher;
    private readonly ILogger<BackChannelAuthenticationUserValidator> _logger;

    public BackChannelAuthenticationUserValidator(
        IQueryDispatchFacade queryDispatcher,
        ILogger<BackChannelAuthenticationUserValidator> logger)
    {
        _queryDispatcher = queryDispatcher;
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
        // We currently use email as the username so this should only be one check.
        var getUserByEmailQuery = new GetUserByEmailQuery(userValidatorContext.LoginHint);
        Result<User> getUserByEmailResult = await _queryDispatcher.DispatchAsync(getUserByEmailQuery);

        if (getUserByEmailResult.HasErrors)
        {
            if (getUserByEmailResult.Errors.ContainsNotFound())
            {
                _logger.LogWarning("User not found for login hint: {LoginHint}.", userValidatorContext.LoginHint);
                result.Error = "invalid_user";
                result.ErrorDescription = "User not found";
                return result;
            }

            result.Error = "invalid_user";
            result.ErrorDescription = getUserByEmailResult.FirstError.Message;
            return result;
        }

        User user = getUserByEmailResult.Value;

        // Custom validation: Check if the user account is enabled
        //if (!user.IsEnabled)
        //{
        //    _logger.LogWarning("User account is disabled: {UserId}.", user.Id);
        //    result.Error = "invalid_request";
        //    result.ErrorDescription = "User account is disabled";
        //    return result;
        //}

        var getUserClaimsQuery = new GetUserClaimsQuery(user.UserId);
        Result<ICollection<Claim>> getUserClaimsResult = await _queryDispatcher.DispatchAsync(getUserClaimsQuery);

        if (getUserClaimsResult.HasErrors)
        {
            if (getUserClaimsResult.Errors.ContainsNotFound())
            {
                _logger.LogWarning("User not found for login hint: {LoginHint}.", userValidatorContext.LoginHint);
                result.Error = "invalid_user";
                result.ErrorDescription = "User not found";
                return result;
            }

            result.Error = "invalid_user_claims";
            result.ErrorDescription = getUserClaimsResult.FirstError.Message;
            return result;
        }

        ICollection<Claim> claims = getUserClaimsResult.Value;

        result.Subject = new ClaimsPrincipal(new ClaimsIdentity(claims));

        if (string.IsNullOrEmpty(userValidatorContext.BindingMessage))
        {
            return result;
        }

        bool isValidBindingMessage = ValidateBindingMessage(userValidatorContext.BindingMessage);

        if (isValidBindingMessage)
        {
            return result;
        }

        _logger.LogWarning("Invalid binding message: {BindingMessage}.", userValidatorContext.BindingMessage);
        result.Error = "invalid_binding_message";
        result.ErrorDescription = "The provided binding message is invalid.";
        return result;

    }

    private static bool ValidateBindingMessage(string bindingMessage)
    {
        // TODO: Implement binding message validation logic here.
        // This is just a placeholder implementation.
        return !string.IsNullOrEmpty(bindingMessage);
    }
}
