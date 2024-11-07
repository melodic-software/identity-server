using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Pages.Account.Login;

public class InputModel
{
    [Required(ErrorMessage = "Please enter your email address.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Please enter your password.")]
    public string Password { get; set; }

    [Display(Name = "Remember me?")]
    public bool RememberLogin { get; set; }
    public string? ReturnUrl { get; set; }
    public string? Button { get; set; }

    public string? DeviceId { get; set; }
}
