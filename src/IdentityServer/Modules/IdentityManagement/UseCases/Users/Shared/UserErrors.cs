using Enterprise.Patterns.ResultPattern.Errors.Model;
using Enterprise.Patterns.ResultPattern.Errors.Model.Typed;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;

public static class UserErrors
{
    public static ValidationError EmailAlreadyRegistered(string email) =>
        Error.Validation(UserErrorCodes.EmailAlreadyRegistered, UserErrorMessages.EmailAlreadyRegistered(email));

    public static ValidationError EmailNotProvided =>
        Error.Validation(UserErrorCodes.EmailNotProvided, UserErrorMessages.EmailNotProvided);

    public static ValidationError ExternalProviderNotProvided =>
        Error.Validation(UserErrorCodes.ExternalProviderNotProvided, UserErrorMessages.ExternalProviderRequired);

    public static ValidationError ExternalProviderIdNotProvided =>
        Error.Validation(UserErrorCodes.ExternalUserIdNotProvided, UserErrorMessages.ExternalUserIdNotProvided);

    public static ValidationError InvalidCredentials =>
        Error.Validation(UserErrorCodes.InvalidCredentials, UserErrorMessages.InvalidCredentials);

    public static ValidationError IsLockedOut =>
        Error.Validation(UserErrorCodes.IsLockedOut, UserErrorMessages.IsLockedOut);

    public static ValidationError IsNotAllowedToSignIn =>
        Error.Validation(UserErrorCodes.IsNotAllowedToSignIn, UserErrorMessages.IsNotAllowedToSignIn);

    public static Error NotFound =>
        Error.NotFound(UserErrorCodes.NotFound, UserErrorMessages.NotFound);

    public static Error NotFoundWithEmail(string email) =>
        Error.NotFound(UserErrorCodes.NotFound, UserErrorMessages.NotFoundWithEmail(email));

    public static Error NotFoundWithId(string userId) =>
        Error.NotFound(UserErrorCodes.NotFound, UserErrorMessages.NotFoundWithId(userId));

    public static Error NotFoundWithIdentityProviderId(string identityId) =>
        Error.NotFound(UserErrorCodes.NotFound, UserErrorMessages.NotFoundWithIdentityProviderId(identityId));

    public static Error NotFoundWithUserName(string userName) =>
        Error.NotFound(UserErrorCodes.NotFound, UserErrorMessages.NotFoundWithUserName(userName));

    public static ValidationError PasswordNotProvided =>
        Error.Validation(UserErrorCodes.PasswordNotProvided, UserErrorMessages.PasswordNotProvided);

    public static ValidationError RequiresConfirmedEmail =>
        Error.Validation(UserErrorCodes.RequiresConfirmedEmail, UserErrorMessages.RequiresConfirmedEmail);
}