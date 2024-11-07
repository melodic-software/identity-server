using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Pages.Validation.Attributes;

/// <summary>
/// Validation attribute to validate the file extensions of uploaded files.
/// </summary>
public sealed class AllowedFileExtensionsAttribute : ValidationAttribute
{
    /// <summary>
    /// A comma-separated list of allowed file extensions.
    /// </summary>
    public string Extensions { get; set; } = string.Empty;

    /// <summary>
    /// Validates the uploaded file's extension.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>A <see cref="ValidationResult"/> indicating whether validation succeeded.</returns>
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Extensions))
        {
            // If no extensions are defined, allow all file types.
            return ValidationResult.Success;
        }

        var allowedExtensions = Extensions.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(ext => ext.Trim().ToLowerInvariant())
            .Select(ext => ext.StartsWith('.') ? ext : "." + ext)
            .ToHashSet();

        if (value == null)
        {
            // If the file is not required and not provided, validation passes.
            return ValidationResult.Success;
        }

        if (value is not IFormFile file)
        {
            // The value is not a file, return an error.
            return new ValidationResult("Invalid file.");
        }

        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!string.IsNullOrEmpty(extension) && allowedExtensions.Contains(extension))
        {
            return ValidationResult.Success;
        }

        string allowedExtensionsFormatted = string.Join(", ", allowedExtensions);

        return new ValidationResult(ErrorMessage ?? $"The file extension must be one of the following: {allowedExtensionsFormatted}");
    }
}