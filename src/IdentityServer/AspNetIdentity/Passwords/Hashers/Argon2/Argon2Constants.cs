namespace IdentityServer.AspNetIdentity.Passwords.Hashers.Argon2;

public static class Argon2Constants
{
    /// <summary>
    /// This is also known as the iteration count.
    /// </summary>
    public static class TimeCosts
    {
        public const int One = 1;
        public const int Two = 2;
        public const int Three = 3;
        public const int Four = 4;
        public const int Five = 5;
        public const int OwaspMinimum = Two;
    }

    public static class MemoryCosts
    {
        public const int SevenMegabytes = 7 * 1024;
        public const int NineMegabytes = 9 * 1024;
        public const int TwelveMegabytes = 12 * 1024;
        public const int NineteenMegabytes = 19 * 1024;
        public const int FortySixMegabytes = 46 * 1024;
        public const int OwaspMinimum = NineteenMegabytes;
    }

    /// <summary>
    /// This is also known as the degree of parallelism.
    /// </summary>
    public static class LanesAndThreads
    {
        public const int OwaspRecommendation = 1;
    }

    public static class HashLengths
    {
        public const int Default = 32; // 256 bits
        public const int Extended = 64; // 512 bits
    }
}