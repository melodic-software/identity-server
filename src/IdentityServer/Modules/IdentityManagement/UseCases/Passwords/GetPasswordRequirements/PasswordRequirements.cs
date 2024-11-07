namespace IdentityServer.Modules.IdentityManagement.UseCases.Passwords.GetPasswordRequirements;

public class PasswordRequirements
{
    public int RequiredLength { get; }
    public int RequiredUniqueChars { get; }
    public bool RequireNonAlphanumeric { get; }
    public bool RequireLowercase { get; }
    public bool RequireUppercase { get; }
    public bool RequireDigit { get; }

    public PasswordRequirements(
        int requiredLength,
        int requiredUniqueChars,
        bool requireNonAlphanumeric,
        bool requireLowercase,
        bool requireUppercase,
        bool requireDigit)
    {
        RequiredLength = requiredLength;
        RequiredUniqueChars = requiredUniqueChars;
        RequireNonAlphanumeric = requireNonAlphanumeric;
        RequireLowercase = requireLowercase;
        RequireUppercase = requireUppercase;
        RequireDigit = requireDigit;
    }
}