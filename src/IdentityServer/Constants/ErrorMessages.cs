using Microsoft.AspNetCore.Authentication;

namespace IdentityServer.Constants;

public static class ErrorMessages
{
    public const string AuthenticationSchemeNotFound = "Authentication scheme was not found.";
    public const string AuthPropertiesNullScheme = "Null scheme in authentication properties.";
    public const string EmailMustBeProvided = "Email must be provided.";
    public const string EmailNotUnique = "An account with this email address has already been registered.";
    public const string ExternalAuthNullPrincipal = "External authentication produced a null principal.";
    public const string ExternalIdentityInvalidEmailAddress = "Email address could not be obtained from external identity.";
    public const string ExternalProviderUnknown = "Unknown external provider.";
    public const string ExternalUserIdUnknown = "Unknown external user ID.";
    public const string InvalidConfirmationUrl = "Confirmation URL is invalid.";
    public const string InvalidLoginHint = "LoginHint is invalid.";
    public const string InvalidReturnUrl = "Invalid return URL.";
    public const string PasswordMustBeProvided = "Password must be provided.";
    public const string ReturnUrlNotProvided = "Return URL was not provided.";
    public const string UnknownUserId = "Unknown user ID.";
    public const string UserMissingEmailAddress = "The user account does not have an email address.";
    public const string UserNotFound = "User not found.";
    
    public static string ExternalAuthenticationError(AuthenticateResult result) => $"External authentication error: {result.Failure}";
}
