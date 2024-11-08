using Enterprise.ApplicationServices.Core.Commands.Model.Pragmatic;
using Enterprise.Patterns.ResultPattern.Model;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Passwords.ChangePassword;

public sealed class ChangePasswordCommand : ICommand<Result>
{
    public string UserId { get; }
    public string CurrentPassword { get; }
    public string NewPassword { get; }

    public ChangePasswordCommand(string userId, string currentPassword, string newPassword)
    {
        UserId = userId;
        CurrentPassword = currentPassword;
        NewPassword = newPassword;
    }
}