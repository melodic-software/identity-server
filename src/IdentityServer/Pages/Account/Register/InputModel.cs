using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Pages.Account.Register;

public class InputModel : IValidatableObject
{
    [MaxLength(320)]
    [Required(ErrorMessage = "Please enter an email address.")]
    [EmailAddress(ErrorMessage = "Please provide a valid e-mail address.")]
    public string? Email { get; set; }

    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    [DisplayName("Confirm Password")]
    public string? ConfirmPassword { get; set; }

    [Required(ErrorMessage = "Please enter your first name.")]
    [DisplayName("First Name")]
    [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "Please enter your last name.")]
    [DisplayName("Last Name")]
    [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters")]
    public string? LastName { get; set; }

    public string? Button { get; set; }

    public string? ExternalProvider { get; set; }
    public string? ExternalUserId { get; set; }
    public bool IsExternalLogin => !string.IsNullOrWhiteSpace(ExternalProvider) && !string.IsNullOrWhiteSpace(ExternalUserId);

    public string? ReturnUrl { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IsExternalLogin)
        {
            yield break;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            yield return new ValidationResult("The Password field is required.", [nameof(Password)]);
        }

        if (string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            yield return new ValidationResult("The Confirm Password field is required.", [nameof(ConfirmPassword)]);
        }

        if (Password != ConfirmPassword)
        {
            yield return new ValidationResult("The password and confirmation password do not match.", [nameof(ConfirmPassword)]);
        }
    }
}
