using Isopoh.Cryptography.Argon2;

namespace IdentityServer.AspNetIdentity.Passwords.Hashers.Argon2;

public class Argon2PasswordHasherOptions
{
    /// <summary>
    /// The number of iterations to perform. Increasing this increases the computation time linearly.
    /// OWASP recommends values between t=2 to t=5.
    /// </summary>
    public int TimeCost { get; set; } = Argon2Constants.TimeCosts.OwaspMinimum;

    /// <summary>
    /// The memory usage in kilobytes. Increasing this increases memory usage exponentially.
    /// OWASP provides configurations ranging from m=7168 (7 MiB) to m=19456 (19 MiB).
    /// </summary>
    public int MemoryCost { get; set; } = Argon2Constants.MemoryCosts.OwaspMinimum;

    /// <summary>
    /// The degree of parallelism. Should be less than or equal to Threads.
    /// OWASP recommends p=1.
    /// </summary>
    public int Lanes { get; set; } = Argon2Constants.LanesAndThreads.OwaspRecommendation;

    /// <summary>
    /// The number of threads to use. Should be less than or equal to the number of CPU cores.
    /// </summary>
    public int Threads { get; set; } = Argon2Constants.LanesAndThreads.OwaspRecommendation;

    /// <summary>
    /// The length of the generated hash in bytes.
    /// OWASP doesn't specify a HashLength, but a common value is 32 bytes (256 bits).
    /// </summary>
    public int HashLength { get; set; } = Argon2Constants.HashLengths.Default;

    /// <summary>
    /// The variant of Argon2 algorithm to use. Argon2id is recommended for password hashing.
    /// </summary>
    public Argon2Type Type { get; set; } = Argon2Type.HybridAddressing;

    /// <summary>
    /// The version of the Argon2 algorithm.
    /// </summary>
    public Argon2Version Version { get; set; } = Argon2Version.Nineteen;
}