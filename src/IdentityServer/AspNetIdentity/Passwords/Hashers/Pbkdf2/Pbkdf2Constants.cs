namespace IdentityServer.AspNetIdentity.Passwords.Hashers.Pbkdf2;

public static class Pbkdf2Constants
{
    public static class Iterations
    {
        public const int OwaspSha1Recommendation = 1300000;
        public const int OwaspSha256Recommendation = 600000;
        public const int OwaspSha512Recommendation = 210000;

        public const int FipsCompliantIterationCount = 600000;
    }
}