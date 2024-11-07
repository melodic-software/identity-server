using IdentityServer.AspNetIdentity.Models;
using Isopoh.Cryptography.Argon2;
using Isopoh.Cryptography.SecureArray;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using IsopohArgon2 = Isopoh.Cryptography.Argon2.Argon2;

namespace IdentityServer.AspNetIdentity.Passwords.Hashers.Argon2;

// https://github.com/mheyman/Isopoh.Cryptography.Argon2?tab=readme-ov-file#usage

public class Argon2PasswordHasher : IPasswordHasher<ApplicationUser>
{
    private readonly Argon2PasswordHasherOptions _options;
    private const int SaltSize = 16; // 128 bits

    public Argon2PasswordHasher(IOptions<Argon2PasswordHasherOptions> options)
    {
        _options = options.Value;
    }

    public string HashPassword(ApplicationUser user, string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        var config = new Argon2Config
        {
            Type = _options.Type,
            Version = _options.Version,
            TimeCost = _options.TimeCost,
            MemoryCost = _options.MemoryCost,
            Lanes = _options.Lanes,
            Threads = _options.Threads,
            // This supports full Unicode, aligning with OWASP's recommendation.
            Password = Encoding.UTF8.GetBytes(password),
            Salt = salt,
            Secret = null,
            AssociatedData = null,
            HashLength = _options.HashLength,
        };

        using var argon2 = new IsopohArgon2(config);

        string hashString;

        using (SecureArray<byte> hash = argon2.Hash())
        {
            hashString = config.EncodeString(hash.Buffer);
        }

        ClearSensitiveDataIn(config);

        return hashString;
    }

    public PasswordVerificationResult VerifyHashedPassword(ApplicationUser user, string hashedPassword, string providedPassword)
    {
        // Ensure the hashed password uses the Argon2id format
        if (!hashedPassword.StartsWith("$argon2id$", StringComparison.Ordinal))
        {
            // Not an Argon2 hash
            return PasswordVerificationResult.Failed;
        }

        // Convert the provided password to bytes (full Unicode support)
        byte[] passwordBytes = Encoding.UTF8.GetBytes(providedPassword);

        // Use Argon2.Verify to check the password against the stored hash
        bool isVerified = IsopohArgon2.Verify(hashedPassword, passwordBytes, _options.Threads);

        return isVerified ? PasswordVerificationResult.Success : PasswordVerificationResult.Failed;
    }

    /// <summary>
    /// This method ensures sensitive data is cleared from memory.
    /// </summary>
    private static void ClearSensitiveDataIn(Argon2Config config)
    {
        if (config.Password != null)
        {
            CryptographicOperations.ZeroMemory(config.Password);
            config.Password = null;
        }

        if (config.Secret != null)
        {
            CryptographicOperations.ZeroMemory(config.Secret);
            config.Secret = null;
        }
    }
}
