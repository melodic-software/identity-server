using Enterprise.Domain.Events.Model.Abstract;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.RegisterUser;

public sealed record UserRegisteredDomainEvent : DomainEvent
{
    public string UserId { get; }
    public string? ReturnUrl { get; }

    public UserRegisteredDomainEvent(string userId, string? returnUrl)
    {
        UserId = userId;
        ReturnUrl = returnUrl;
    }
}