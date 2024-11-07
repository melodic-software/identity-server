namespace IdentityServer.Modules.IdentityManagement.UseCases.Passwords.ResetPassword;

public class ResetPasswordRequest
{
    public string UserId { get; set; }
    public string PasswordResetToken { get; set; }
    public string NewPassword { get; set; }
}