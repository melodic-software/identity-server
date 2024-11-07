using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace IdentityServer.Pages.Validation.Attributes;

public sealed class CompareFirstLetterAttribute : ValidationAttribute
{
    public string OtherProperty { get; set; }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        PropertyInfo? otherPropertyInfo = validationContext.ObjectType.GetRuntimeProperty(OtherProperty);

        if (otherPropertyInfo == null)
        {
            return new ValidationResult("You must specify another property to compare to.");
        }

        object? otherValue = otherPropertyInfo.GetValue(validationContext.ObjectInstance, null);

        if (value == null && otherValue == null)
        {
            return ValidationResult.Success;
        }

        char? valueFirst = value?.ToString()?.ToLower(CultureInfo.InvariantCulture).FirstOrDefault();
        char? otherValueFirst = otherValue?.ToString()?.ToLower(CultureInfo.InvariantCulture).FirstOrDefault();

        if (valueFirst != null && otherValueFirst != null && valueFirst != otherValueFirst) {
            return new ValidationResult(ErrorMessage ?? $"The first letters of {validationContext.DisplayName} and {otherPropertyInfo.Name} must match");
        }

        return ValidationResult.Success;
    }
}