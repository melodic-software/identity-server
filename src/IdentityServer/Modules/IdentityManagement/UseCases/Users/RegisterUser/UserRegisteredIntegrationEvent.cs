using Enterprise.Events.Integration.Model;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.RegisterUser;

public sealed record UserRegisteredIntegrationEvent : IntegrationEvent
{
    public string UserId { get; init; }
    public string? Email { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }

    public UserRegisteredIntegrationEvent(string userId, string? email, string firstName, string lastName)
    {
        UserId = userId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
    }
}