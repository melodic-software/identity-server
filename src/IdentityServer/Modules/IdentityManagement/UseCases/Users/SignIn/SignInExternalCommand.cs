using Enterprise.ApplicationServices.Core.Commands.Model.Pragmatic;
using Enterprise.Patterns.ResultPattern.Model.Generic;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.SignIn;

public class SignInExternalCommand : ICommand<Result<ExternalSignIn>>
{
    public string UserId { get; }
    public string? ExternalProvider { get; }
    public string? ExternalUserId { get; }
    public string? ReturnUrl { get; }

    public SignInExternalCommand(string userId, string? externalProvider, string? externalUserId, string? returnUrl)
    {
        UserId = userId;
        ExternalProvider = externalProvider;
        ExternalUserId = externalUserId;
        ReturnUrl = returnUrl;
    }
}
