namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;

public static class UserErrorCodes
{
    public const string EmailAlreadyRegistered = "User.EmailAlreadyRegistered";
    public const string EmailNotProvided = "User.EmailNotProvided";
    public const string ExternalProviderNotProvided = "User.ExternalProviderNotProvided";
    public const string ExternalUserIdNotProvided = "User.ExternalUserIdNotProvided";
    public const string InvalidCredentials = "User.InvalidCredentials";
    public const string IsLockedOut = "User.LockedOut";
    public const string IsNotAllowedToSignIn = "User.NotAllowedToSignIn";
    public const string NotFound = "User.NotFound";
    public const string PasswordNotProvided = "User.PasswordNotProvided";
    public const string RequiresConfirmedEmail = "User.RequiresConfirmedEmail";
}