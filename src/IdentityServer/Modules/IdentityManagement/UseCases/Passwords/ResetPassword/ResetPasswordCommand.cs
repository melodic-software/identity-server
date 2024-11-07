using Enterprise.ApplicationServices.Core.Commands.Model.Pragmatic;
using Enterprise.Patterns.ResultPattern.Model;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Passwords.ResetPassword;

public class ResetPasswordCommand : ICommand<Result>
{
    public string UserId { get; }
    public string? PasswordResetToken { get; }
    public string NewPassword { get; }

    public ResetPasswordCommand(string userId, string? passwordResetToken, string newPassword)
    {
        UserId = userId;
        PasswordResetToken = passwordResetToken;
        NewPassword = newPassword;
    }
}