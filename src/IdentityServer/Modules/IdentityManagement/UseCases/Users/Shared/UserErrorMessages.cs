namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;

public static class UserErrorMessages
{
    public static string EmailAlreadyRegistered(string email) => $"The email {email} has already been registered.";

    public const string EmailNotProvided = "Email must be provided.";
    public const string ExternalProviderRequired = "External provider is required.";
    public const string ExternalUserIdNotProvided = "External user ID must be provided.";
    public const string InvalidCredentials = "Credentials are invalid.";
    public const string IsLockedOut = "The user is locked out.";
    public const string IsNotAllowedToSignIn = "The user is not allowed to sign in.";
    public const string NotFound = "The user was not found.";

    public static string NotFoundWithEmail(string emailAddress) => $"The user with the email address \"{emailAddress}\" was not found.";
    public static string NotFoundWithId(string userId) => $"A user with the identifier \"{userId}\" was not found.";
    public static string NotFoundWithIdentityProviderId(string identityId) => $"The user with the IDP identifier \"{identityId}\" was not found.";
    public static string NotFoundWithUserName(string userName) => $"The user with the user name \"{userName}\" was not found.";

    public const string PasswordNotProvided = "Password must be provided.";
    public const string RequiresConfirmedEmail = "The email address of this account has not yet been confirmed.";
}