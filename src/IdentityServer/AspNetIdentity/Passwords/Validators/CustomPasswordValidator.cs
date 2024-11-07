using IdentityServer.AspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.AspNetIdentity.Passwords.Validators;

// NOTE: This is only used in these specific operations:
// User Registration (Sign-Up): When a new user account is created with a password.
// Password Change: When a logged-in user changes their password.
// Password Reset: When a user resets their password via a forgotten password flow.
// Administrator Actions: When an admin sets or resets a user's password.

public class CustomPasswordValidator : PasswordValidator<ApplicationUser>
{
    public const int MaxPasswordLength = 64;

    public override async Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string? password)
    {
        // Use the built-in configuration and validation.
        IdentityResult identityResult = await base.ValidateAsync(manager, user, password);

        // We're just extending this to check for max password length, and other non-configurable options (as of 2024).
        // Get errors (if any) that the base implementation has reported.
        var errors = identityResult.Errors.ToList();

        // NIST recommends a maximum password length of 64 characters (Section 5.1.1.1).
        // This still allows of password phrases to be used.
        // TODO: Use configuration or at least a constant value.
        // TODO: This is the bare minimum, we could consider increasing this if needed.
        if (password is { Length: > MaxPasswordLength })
        {
            identityResult = IdentityResult.Failed(errors.ToArray());

            errors.Add(new IdentityError
            {
                Code = "Password.TooLong",
                Description = "Password cannot be greater than 64 characters."
            });
        }

        // Enforce maximum password length for bcrypt.
        // NOTE: If the previous length restriction is in place at the recommended cap of 64 characters, this check is not needed.
        //if (!string.IsNullOrWhiteSpace(password) && IsBcryptUsed() && Encoding.UTF8.GetByteCount(password) > 72)
        //{
        //    errors.Add(new IdentityError
        //    {
        //        Code = "Password.TooLongForBcrypt",
        //        Description = "Password is too long. Maximum length is 72 characters."
        //    });
        //}

        // TODO: Check if validation has failed based on characters.
        //  NIST encourages allowing all printable ASCII characters, including spaces,
        // and supporting Unicode characters to enable the use of passphrases and accommodate users from different locales (Section 5.1.1.2).

        // TODO: Implement password blacklisting.
        // NIST recommends screening passwords against a list of commonly used, expected, or compromised passwords (Section 5.1.1.2).
        // https://haveibeenpwned.com/Passwords
        // https://github.com/HaveIBeenPwned/PwnedPasswordsDownloader

        return identityResult;
    }
}