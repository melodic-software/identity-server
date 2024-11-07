using Enterprise.ApplicationServices.Core.Commands.Model.Pragmatic;
using Enterprise.Patterns.ResultPattern.Model;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Emails.ConfirmEmailChange;

public sealed class ConfirmEmailChangeCommand : ICommand<Result>
{
    public string UserId { get; }
    public string Email { get; }
    public string Code { get; }

    public ConfirmEmailChangeCommand(string userId, string email, string code)
    {
        UserId = userId;
        Email = email;
        Code = code;
    }
}