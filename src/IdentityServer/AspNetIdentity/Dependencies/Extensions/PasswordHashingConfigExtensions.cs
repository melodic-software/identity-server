using IdentityServer.AspNetIdentity.Models;
using IdentityServer.AspNetIdentity.Passwords.Hashers.Argon2;
using IdentityServer.AspNetIdentity.Passwords.Hashers.BCrypt;
using IdentityServer.AspNetIdentity.Passwords.Hashers.Composite;
using IdentityServer.AspNetIdentity.Passwords.Hashers.Pbkdf2;
using IdentityServer.Constants;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.AspNetIdentity.Dependencies.Extensions;

public static class PasswordHashingConfigExtensions
{
    // TODO: Review these articles:
    // https://dropbox.tech/security/how-dropbox-securely-stores-your-passwords
    // https://crypto.stackexchange.com/questions/42415/dropbox-password-security

    public static IServiceCollection ConfigurePasswordHashing(this IServiceCollection services, IConfiguration configuration)
    {
        bool fipsComplianceRequired = configuration.GetValue(ConfigurationKeys.FipsComplianceRequired, false);

        // Configure password hasher options.
        services.Configure<Argon2PasswordHasherOptions>(configuration.GetSection(ConfigurationKeys.Argon2PasswordHasher));
        services.Configure<BCryptPasswordHasherOptions>(configuration.GetSection(ConfigurationKeys.BCryptPasswordHasher));
        services.Configure<PasswordHasherOptions>(options =>
        {
            // The default ASP.NET Core Identity password hasher uses PBKDF2 with HMAC-SHA256, a 128-bit salt, a 256-bit subkey (hash), and 100,000 iterations.
            // PBKDF2 has generally been considered "good enough", assuming you use a high number of iterations and a SHA2 family hash function.
            // It is also FIPS compliant and recommended by NIST (you'll be able to find FIPS-140 validated implementations).
            // However, it is not so secure against newer attack vectors, such as GPU-based attacks,
            // and as a result, it is often considered weak compared to alternatives such as bcrypt and Argon2.
            // In fact, to defend against modern attacks as of 2021, cryptographers suggest that you need to use 310,000 iterations for PBKDF2-HMAC-SHA256.
            // This would make PBKDF2 comparable to bcrypt work factor 8, and it is a little different from ASP.NET Identity's default.

            // If the implementation doesn't handle pre-hashing efficiently, very long passwords could be used to cause a DoS attack.
            // The default implementation handles pre-hashing efficiently, mitigating the DoS risk.

            // https://code-maze.com/aspnetcore-default-asp-net-core-identity-password-hasher

            // This should be increased over time as CPU power evolves.
            int iterationCount = configuration.GetValue(ConfigurationKeys.PasswordHasherIterationCount, Pbkdf2Constants.Iterations.OwaspSha256Recommendation);

            // https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html#introduction
            if (fipsComplianceRequired && iterationCount < Pbkdf2Constants.Iterations.FipsCompliantIterationCount)
            {
                iterationCount = Pbkdf2Constants.Iterations.FipsCompliantIterationCount;
            }

            options.IterationCount = iterationCount;
        });

        // Register individual password hashers.
        services.AddSingleton<Argon2PasswordHasher>();
        services.AddSingleton<BCryptPasswordHasher>();
        services.AddSingleton<PasswordHasher<ApplicationUser>>(); // For PBKDF2

        // This is the password hasher we use that works with multiple implementations.
        services.AddSingleton<IPasswordHasher<ApplicationUser>>(provider =>
        {
            // We support these hashing algorithms, with Argon2 currently taking priority.
            // New users, and password changes will be converted to use it.
            // Existing users will be migrated as they log in.

            Argon2PasswordHasher argon2Hasher = provider.GetRequiredService<Argon2PasswordHasher>();
            BCryptPasswordHasher bcryptHasher = provider.GetRequiredService<BCryptPasswordHasher>();
            PasswordHasher<ApplicationUser> pbkdf2Hasher = provider.GetRequiredService<PasswordHasher<ApplicationUser>>();
            ILogger<CompositePasswordHasher> logger = provider.GetRequiredService<ILogger<CompositePasswordHasher>>();

            IEnumerable<IPasswordHasher<ApplicationUser>> passwordHashers = [argon2Hasher, bcryptHasher, pbkdf2Hasher];

            // https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html#introduction
            if (fipsComplianceRequired)
            {
                // TODO: Is BCrypt OK here?
                passwordHashers = [pbkdf2Hasher];
            }

            return new CompositePasswordHasher(passwordHashers, logger);
        });

        return services;
    }
}