namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;

public class User
{
    public string UserId { get; }
    public string? Email { get; }
    public bool EmailConfirmed { get; }
    public string FirstName { get; }
    public string LastName { get; }

    public User(string userId, string? email, bool emailConfirmed, string firstName, string lastName)
    {
        UserId = userId;
        Email = email;
        EmailConfirmed = emailConfirmed;
        FirstName = firstName;
        LastName = lastName;
    }
}
