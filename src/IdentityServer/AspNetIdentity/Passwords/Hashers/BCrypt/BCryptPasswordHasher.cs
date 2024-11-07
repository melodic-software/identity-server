using BCrypt.Net;
using IdentityServer.AspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Bcrypt = BCrypt.Net.BCrypt;

namespace IdentityServer.AspNetIdentity.Passwords.Hashers.BCrypt;

// https://github.com/BcryptNet/bcrypt.net
// https://www.scottbrady91.com/aspnet-identity/improving-the-aspnet-core-identity-password-hasher
// https://github.com/scottbrady91/ScottBrady91.AspNetCore.Identity.BCryptPasswordHasher

public class BCryptPasswordHasher : IPasswordHasher<ApplicationUser>
{
    private readonly int _workFactor;
    private readonly bool _enhancedEntropy;
    private readonly HashType _hashType;

    public BCryptPasswordHasher(IOptions<BCryptPasswordHasherOptions> options)
    {
        _workFactor = options.Value.WorkFactor;
        _enhancedEntropy = options.Value.EnhancedEntropy;
        _hashType = options.Value.HashType;
    }

    public string HashPassword(ApplicationUser user, string password)
    {
        return Bcrypt.EnhancedHashPassword(password, _hashType, _workFactor);
    }

    public PasswordVerificationResult VerifyHashedPassword(ApplicationUser user, string hashedPassword, string providedPassword)
    {
        bool isValid = Bcrypt.EnhancedVerify(providedPassword, hashedPassword, _hashType);

        // Ensures that passwords are rehashed when the work factor increases.
        bool needsRehash = Bcrypt.PasswordNeedsRehash(hashedPassword, _workFactor);

        if (isValid && needsRehash)
        {
            return PasswordVerificationResult.SuccessRehashNeeded;
        }

        return isValid ? PasswordVerificationResult.Success : PasswordVerificationResult.Failed;
    }
}
