using Enterprise.ApplicationServices.Core.Commands.Model.Pragmatic;
using Enterprise.Patterns.ResultPattern.Model;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Passwords.SendPasswordResetEmail;

public class SendPasswordResetEmailCommand : ICommand<Result>
{
    public string UserId { get; }

    public SendPasswordResetEmailCommand(string userId)
    {
        UserId = userId;
    }
}