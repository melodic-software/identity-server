namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.RegisterUser;

public class RegisterUserRequest
{
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsExternalLogin { get; set; }
    public string? ExternalProvider { get; set; }
    public string? ExternalUserId { get; set; }
    public string? ReturnUrl { get; set; }
}