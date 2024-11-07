namespace IdentityServer.Pages.Account.Login;

public static class LoginOptions
{
    public static readonly bool AllowLocalLogin = true;
    public static readonly bool AllowRememberLogin = true;
    public static readonly TimeSpan RememberMeLoginDuration = TimeSpan.FromDays(30);
    public static readonly string InvalidCredentialsErrorMessage = "Invalid email address or password.";
    public static readonly string InvalidUserIdErrorMessage = "User ID is invalid.";
}
