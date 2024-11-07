using Enterprise.Domain.Events.Model.Abstract;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Emails.ConfirmEmailChange;

public record EmailChangeConfirmedDomainEvent : DomainEvent
{
    public string UserId { get; }
    public string? PreviousEmail { get; }
    public string NewEmail { get; }

    public EmailChangeConfirmedDomainEvent(string userId, string? previousEmail, string newEmail)
    {
        UserId = userId;
        PreviousEmail = previousEmail;
        NewEmail = newEmail;
    }
}