using Enterprise.ApplicationServices.Core.Commands.Model.Pragmatic;
using Enterprise.Patterns.ResultPattern.Model.Generic;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.SignIn;

public class SignInLocalCommand : ICommand<Result<SignInLocalResult>>
{
    public string? UserName { get; }
    public string? Email { get; }
    public string Password { get; }
    public bool RememberLogin { get; }
    public string? ReturnUrl { get; }
    public string? DeviceId { get; }

    public SignInLocalCommand(string? userName, string? email, string password, bool rememberLogin, string? returnUrl, string? deviceId)
    {
        UserName = userName;
        Email = email;
        Password = password;
        RememberLogin = rememberLogin;
        ReturnUrl = returnUrl;
        DeviceId = deviceId;
    }
}