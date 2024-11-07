using static IdentityServer.AspNetIdentity.Passwords.Hashers.Composite.AlgorithmIdentifierConstants;
using static IdentityServer.AspNetIdentity.Passwords.Hashers.Composite.PasswordHasherDictionaryKeys;
using static System.StringComparison;

namespace IdentityServer.AspNetIdentity.Passwords.Hashers.Composite;

public static class PasswordHasherDictionaryKeyService
{
    public static string GetKeyFor(string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword))
        {
            return null;
        }

        if (hashedPassword.StartsWith(Argon2IdPrefix, Ordinal))
        {
            return Argon2DictionaryKey;
        }

        if (hashedPassword.StartsWith(BCrypt2APrefix, Ordinal) ||
            hashedPassword.StartsWith(BCrypt2BPrefix, Ordinal) ||
            hashedPassword.StartsWith(BCrypt2YPrefix, Ordinal))
        {
            return BCryptDictionaryKey;
        }

        if (IsPbkdf2Hash(hashedPassword))
        {
            return Pbkdf2DictionaryKey;
        }

        return null;
    }

    private static bool IsPbkdf2Hash(string hashedPassword)
    {
        // Ensures that the hash is at least long enough to be a valid PBKDF2 hash.
        if (hashedPassword.Length < 24)
        {
            return false;
        }

        try
        {
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);

            // Check version byte (should be 0x00 or 0x01).
            // The default PBKDF2 hashes in ASP.NET Identity have a version byte at the beginning (0x00 or 0x01).
            byte version = hashBytes[0];

            if (version != 0x00 && version != 0x01)
            {
                return false;
            }

            // Additional checks can be added here, such as verifying the length of the hash.

            return true;
        }
        catch
        {
            return false;
        }
    }
}