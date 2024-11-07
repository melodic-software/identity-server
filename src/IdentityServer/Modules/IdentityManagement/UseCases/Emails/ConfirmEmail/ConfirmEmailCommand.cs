using Enterprise.ApplicationServices.Core.Commands.Model.Pragmatic;
using Enterprise.Patterns.ResultPattern.Model;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Emails.ConfirmEmail;

public class ConfirmEmailCommand : ICommand<Result>
{
    public string UserId { get; }
    public string Token { get; }

    public ConfirmEmailCommand(string userId, string token)
    {
        UserId = userId;
        Token = token;
    }
}