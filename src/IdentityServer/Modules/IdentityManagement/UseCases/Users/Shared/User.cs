namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;

public class User
{
    public string UserId { get; }
    public string? Username { get; }
    public string? Email { get; }
    public bool EmailConfirmed { get; }
    public string? PhoneNumber { get; }
    public string FirstName { get; }
    public string LastName { get; }

    public User(string userId, string? username, string? email, bool emailConfirmed, string? phoneNumber, string firstName, string lastName)
    {
        UserId = userId;
        Username = username;
        Email = email;
        EmailConfirmed = emailConfirmed;
        PhoneNumber = phoneNumber;
        FirstName = firstName;
        LastName = lastName;
    }
}
