using Enterprise.Domain.Events.Model.Abstract;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Emails.SendConfirmationEmail;

public record ConfirmationEmailSentDomainEvent : DomainEvent
{
    public string UserId { get; }
    public string Email { get; }

    public ConfirmationEmailSentDomainEvent(string userId, string email)
    {
        UserId = userId;
        Email = email;
    }
}