using Enterprise.ApplicationServices.Core.Commands.Model.Pragmatic;
using Enterprise.Patterns.ResultPattern.Model.Generic;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Emails.SendConfirmationEmail;

public class SendConfirmationEmailCommand : ICommand<Result<string>>
{
    public string UserId { get; }
    public string? ReturnUrl { get; }

    public SendConfirmationEmailCommand(string userId, string? returnUrl)
    {
        UserId = userId;
        ReturnUrl = returnUrl;
    }
}