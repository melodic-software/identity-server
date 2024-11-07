using Enterprise.Domain.Events.Model.Abstract;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Passwords.ResetPassword;

public sealed record PasswordResetDomainEvent : DomainEvent
{
    public string UserId { get; }

    public PasswordResetDomainEvent(string userId)
    {
        UserId = userId;
    }
}
