using Azure.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;

namespace IdentityServer.AspNetIdentity.Email.Senders.Microsoft365;

public class Microsoft365EmailSender : IEmailSender
{
    private readonly Microsoft365Settings _settings;
    private readonly ILogger<Microsoft365EmailSender> _logger;

    public Microsoft365EmailSender(IOptions<Microsoft365Settings> settings, ILogger<Microsoft365EmailSender> logger)
    {
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var clientSecretCredential = new ClientSecretCredential(
            _settings.TenantId,
            _settings.ClientId,
            _settings.ClientSecret
        );

        using var graphClient = new GraphServiceClient(clientSecretCredential);

        var message = new Message
        {
            Subject = subject,
            Body = new ItemBody
            {
                ContentType = BodyType.Html,
                Content = htmlMessage
            },
            ToRecipients =
            [
                new()
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = email
                    }
                }
            ]
        };

        var sendMailPostRequestBody = new SendMailPostRequestBody
        {
            Message = message,
            SaveToSentItems = true
        };

        try
        {
            // Use the specific mailbox instead of 'Me'.
            // TODO: Use constant.
            await graphClient.Users[_settings.GraphUserEmailAddress]
                .SendMail
                .PostAsync(sendMailPostRequestBody);

            // TODO: Move to decorator.
            _logger.LogInformation("Email sent successfully.");
        }
        catch (ServiceException ex)
        {
            // TODO: Should we be concerned about PII in this response body?
            _logger.LogError(ex, "Error sending email: {RawResponseBody}", ex.RawResponseBody);
            throw;
        }
    }
}
