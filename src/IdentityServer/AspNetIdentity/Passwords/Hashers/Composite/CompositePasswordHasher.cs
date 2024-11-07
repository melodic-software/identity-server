using IdentityServer.AspNetIdentity.Models;
using IdentityServer.AspNetIdentity.Passwords.Hashers.Argon2;
using IdentityServer.AspNetIdentity.Passwords.Hashers.BCrypt;
using Microsoft.AspNetCore.Identity;
using static IdentityServer.AspNetIdentity.Passwords.Hashers.Composite.PasswordHasherDictionaryKeys;

namespace IdentityServer.AspNetIdentity.Passwords.Hashers.Composite;

public class CompositePasswordHasher : IPasswordHasher<ApplicationUser>
{
    private readonly ILogger<CompositePasswordHasher> _logger;
    private readonly Dictionary<string, IPasswordHasher<ApplicationUser>> _hashers;

    public CompositePasswordHasher(
        IEnumerable<IPasswordHasher<ApplicationUser>> hashers,
        ILogger<CompositePasswordHasher> logger)
    {
        _logger = logger;
        _hashers = new Dictionary<string, IPasswordHasher<ApplicationUser>>();

        foreach (IPasswordHasher<ApplicationUser> hasher in hashers)
        {
            switch (hasher)
            {
                case Argon2PasswordHasher:
                    _hashers.Add(Argon2DictionaryKey, hasher);
                    break;
                case BCryptPasswordHasher:
                    _hashers.Add(BCryptDictionaryKey, hasher);
                    break;
                case PasswordHasher<ApplicationUser>:
                    _hashers.Add(Pbkdf2DictionaryKey, hasher);
                    break;
            }
        }
    }

    public string HashPassword(ApplicationUser user, string password)
    {
        // Always use the most secure hasher (e.g., Argon2).
        IPasswordHasher<ApplicationUser> hasher = GetMostSecureHasher();
        return hasher.HashPassword(user, password);
    }

    public PasswordVerificationResult VerifyHashedPassword(ApplicationUser user, string hashedPassword, string providedPassword)
    {
        string dictionaryKey = PasswordHasherDictionaryKeyService.GetKeyFor(hashedPassword);

        if (string.IsNullOrEmpty(dictionaryKey) || !_hashers.TryGetValue(dictionaryKey, out IPasswordHasher<ApplicationUser> hasher))
        {
            _logger.LogWarning(
                "Unknown or unsupported password hashing algorithm. " +
                "Algorithm: {Algorithm}, UserId: {UserId}.",
                dictionaryKey, user.Id
            );

            return PasswordVerificationResult.Failed;
        }

        try
        {
            PasswordVerificationResult result = hasher.VerifyHashedPassword(user, hashedPassword, providedPassword);

            IPasswordHasher<ApplicationUser> mostSecureHasher = GetMostSecureHasher();

            if (result != PasswordVerificationResult.Success || hasher == mostSecureHasher)
            {
                return result;
            }

            string previousHasherTypeName = hasher.GetType().Name;
            string newHasherTypeName = mostSecureHasher.GetType().Name;

            _logger.LogWarning("Password has been verified, but needs to be rehashed. It was generated using an older algorithm.");
            _logger.LogInformation("Previous: {PreviousHasherType}, New: {NewHasherType}", previousHasherTypeName, newHasherTypeName);

            return PasswordVerificationResult.SuccessRehashNeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying password for user {UserId}.", user.Id);
            return PasswordVerificationResult.Failed;
        }
    }

    private IPasswordHasher<ApplicationUser> GetMostSecureHasher()
    {
        if (_hashers.TryGetValue(Argon2DictionaryKey, out IPasswordHasher<ApplicationUser>? argon2Hasher))
        {
            return argon2Hasher;
        }

        if (_hashers.TryGetValue(BCryptDictionaryKey, out IPasswordHasher<ApplicationUser>? bcryptHasher))
        {
            return bcryptHasher;
        }

        if (_hashers.TryGetValue(Pbkdf2DictionaryKey, out IPasswordHasher<ApplicationUser>? pbkdf2Hasher))
        {
            return pbkdf2Hasher;
        }

        throw new InvalidOperationException("The preferred secure password hasher has not been configured.");
    }
}