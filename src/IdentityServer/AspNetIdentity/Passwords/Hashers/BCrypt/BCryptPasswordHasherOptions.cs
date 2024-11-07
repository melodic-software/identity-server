using BCrypt.Net;

namespace IdentityServer.AspNetIdentity.Passwords.Hashers.BCrypt;

/// <summary>
/// Options for configuring the BCrypt password hasher.
/// </summary>
public class BCryptPasswordHasherOptions
{
    /// <summary>
    /// The computational cost of the hashing algorithm. Higher values increase security but require more processing time.
    /// </summary>
    public int WorkFactor { get; set; } = BCryptConstants.WorkFactors.OwaspMinimum;

    /// <summary>
    /// Indicates whether to use enhanced entropy (salt) in hashing.
    /// </summary>
    public bool EnhancedEntropy { get; set; } = true;

    /// <summary>
    /// The hash type to use when enhanced entropy is enabled.
    /// </summary>
    public HashType HashType { get; set; } = HashType.SHA512;
}