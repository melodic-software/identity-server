using Enterprise.ApplicationServices.Core.Commands.Model.Pragmatic;
using Enterprise.Patterns.ResultPattern.Model.Generic;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.RegisterUser;

public sealed record RegisterUserCommand : ICommand<Result<string>>
{
    public string? UserId { get; }
    public string? Email { get; }
    public string? Password { get; }
    public string? FirstName { get; }
    public string? LastName { get; }
    public bool IsExternalLogin { get; }
    public string? ExternalProvider { get; }
    public string? ExternalUserId { get; }
    public string? ReturnUrl { get; }

    public RegisterUserCommand(string? id,
        string? email,
        string? password,
        string? firstName,
        string? lastName,
        bool isExternalLogin,
        string? externalProvider,
        string? externalUserId,
        string? returnUrl)
    {
        UserId = id;
        Email = email;
        Password = password;
        FirstName = firstName;
        LastName = lastName;
        IsExternalLogin = isExternalLogin;
        ExternalProvider = externalProvider;
        ExternalUserId = externalUserId;
        ReturnUrl = returnUrl;
    }
}