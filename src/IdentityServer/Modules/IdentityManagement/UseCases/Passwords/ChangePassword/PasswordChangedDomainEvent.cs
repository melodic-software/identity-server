using Enterprise.Domain.Events.Model.Abstract;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Passwords.ChangePassword;

public record PasswordChangedDomainEvent : DomainEvent
{
    public string UserId { get; }

    public PasswordChangedDomainEvent(string userId)
    {
        UserId = userId;
    }
}