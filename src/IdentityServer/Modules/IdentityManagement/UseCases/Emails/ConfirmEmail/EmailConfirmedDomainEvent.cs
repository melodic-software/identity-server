using Enterprise.Domain.Events.Model.Abstract;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Emails.ConfirmEmail;

public record EmailConfirmedDomainEvent : DomainEvent
{
    public string UserId { get; }
    public string Email { get; }

    public EmailConfirmedDomainEvent(string userId, string email)
    {
        UserId = userId;
        Email = email;
    }
}