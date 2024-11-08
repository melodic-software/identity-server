using Enterprise.ApplicationServices.Core.Commands.Model.Pragmatic;
using Enterprise.Patterns.ResultPattern.Model.Generic;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Emails.SendEmailChangeConfirmationEmail;

public class SendEmailChangeConfirmationEmailCommand : ICommand<Result<string>>
{
    public string UserId { get; }
    public string NewEmail { get; }

    public SendEmailChangeConfirmationEmailCommand(string userId, string newEmail)
    {
        UserId = userId;
        NewEmail = newEmail;
    }
}