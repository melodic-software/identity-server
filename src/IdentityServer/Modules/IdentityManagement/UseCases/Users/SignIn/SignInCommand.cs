
using Enterprise.ApplicationServices.Core.Commands.Model.Pragmatic;
using Enterprise.Patterns.ResultPattern.Model.Generic;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.SignIn;

public class SignInCommand : ICommand<Result<bool>>
{
    public string UserId { get; }
    public string? ExternalProvider { get; }
    public string? ExternalUserId { get; }
    public bool RememberLogin { get; }
    public string? ReturnUrl { get; }

    public SignInCommand(string userId, string? externalProvider, string? externalUserId, bool rememberLogin, string? returnUrl)
    {
        UserId = userId;
        ExternalProvider = externalProvider;
        ExternalUserId = externalUserId;
        RememberLogin = rememberLogin;
        ReturnUrl = returnUrl;
    }
}
