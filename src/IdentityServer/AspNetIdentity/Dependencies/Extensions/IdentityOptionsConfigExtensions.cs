using Microsoft.AspNetCore.Identity;

namespace IdentityServer.AspNetIdentity.Dependencies.Extensions;

public static class IdentityOptionsConfigExtensions
{
    public static IServiceCollection ConfigureIdentityOptions(this IServiceCollection services)
    {
        return services.Configure<IdentityOptions>(options =>
        {
            // Lockout settings.
            // A successful authentication resets the failed access attempts count and resets the clock.
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 10;
            options.Lockout.AllowedForNewUsers = false;

            // Password settings.
            // NIST = National Institute of Standards and Technology.
            // https://nvlpubs.nist.gov/nistpubs/SpecialPublications/NIST.SP.800-63b.pdf
            // Alternate link: https://doi.org/10.6028/NIST.SP.800-63b
            // The following configuration is recommended best practice as of 10/6/2024.

            // NIST advises against enforcing character composition rules such as
            // requiring uppercase letters, numbers, or special characters (Section 5.1.1.2, Memorized Secret Verifiers).
            // These requirements can lead to predictable password patterns and user frustration.
            // https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html#implement-proper-password-strength-controls
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredUniqueChars = 0;

            // NIST recommends a minimum password length of at least 8 characters (Section 5.1.1.1, Memorized Secret Authenticators).
            // Longer passwords or passphrases enhance security by increasing entropy.
            // The best practice recommendation is 15 characters or more, but that is not the minimum required.
            options.Password.RequiredLength = 8;

            // The following are covered in our custom password validator.
            // NIST recommends supporting passwords of up to 64 characters to accommodate passphrases (Section 5.1.1.1).

            // TODO: Implement password blacklisting.
            // NIST recommends screening passwords against a list of commonly used, expected, or compromised passwords (Section 5.1.1.2).
            // https://haveibeenpwned.com/Passwords
            // https://github.com/HaveIBeenPwned/PwnedPasswordsDownloader

            // The following are covered in our custom composite password hasher.
            // NIST advises using memory-hard hashing algorithms like Argon2 or bcrypt for password storage (Section 5.1.1.2).
            // By default, ASP.NET Identity uses PBKDF2. This custom password hasher uses the current recommendations as of 2024.

            // These next ones are things we're going to have to handle outside the ASP.NET identity framework.

            // Avoid mandatory periodic password changes unless there is evidence of compromise.
            // NIST discourages routine password expiration (Section 5.1.1.2).
            // TODO: Allow for a UI to require password resets for all users or a target set of users when security events occur.

            // NIST recommends rate limiting to protect against online guessing attacks (Section 5.2.2).
            // This could be handled in an application gateway / reverse proxy.
            // https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-8.0
            // TODO: In addition to lockout settings, implement rate limiting and monitor for suspicious activities.

            // NIST discourages the use of security questions and other knowledge-based authentication methods (Section 5.3.1).
            // TODO: Ensure that account recovery processes use secure methods like email-based password resets.
            // Remove any security questions and rely on verified email or phone for account recovery.

            // MFA adds an extra layer of security (Section 5.1.2).
            // We've actually covered this for local accounts with the use of Authsignal.

            // Real-time feedback helps users create stronger passwords (Section 5.1.1.2).
            // We've implemented this in the UI.
            // We could look at swapping out our custom implementation with: https://github.com/dropbox/zxcvbn
            // https://github.com/trichards57/zxcvbn (typescript port)
            // https://github.com/zxcvbn-ts/zxcvbn (another typescript port)
            // https://github.com/trichards57/zxcvbn-cs (.NET port)
            // https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html#implement-proper-password-strength-controls
            // https://zxcvbn-ts.github.io/zxcvbn/guide/getting-started/#as-script-tag

            // SignIn settings.
            options.SignIn.RequireConfirmedEmail = true;
            options.SignIn.RequireConfirmedPhoneNumber = false;
            options.SignIn.RequireConfirmedAccount = true;

            // Token settings.
            // Enable two-factor authentication.
            options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
            options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;

            // User settings.
            // We do not restrict characters in usernames.
            // This enhances accessibility for users worldwide.
            options.User.AllowedUserNameCharacters = string.Empty;
            //options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = true;
        });
    }
}