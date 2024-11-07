using Enterprise.Domain.Events.Model.Abstract;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Passwords.SendPasswordResetEmail;

public sealed record PasswordResetEmailSentDomainEvent : DomainEvent
{
    public string UserId { get; }

    public PasswordResetEmailSentDomainEvent(string userId)
    {
        UserId = userId;
    }
}
