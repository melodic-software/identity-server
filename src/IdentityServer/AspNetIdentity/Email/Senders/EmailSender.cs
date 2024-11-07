using IdentityServer.Constants;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace IdentityServer.AspNetIdentity.Email.Senders;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public EmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // TODO: Make this part of an enterprise library?
        // TODO: Add configuration/options around this?

        IConfigurationSection smtpSettings = _configuration.GetSection("Smtp");

        string? host = smtpSettings.GetValue<string>("Host");
        int port = smtpSettings.GetValue("Port", 25);

        using var client = new SmtpClient(host, port);

        client.EnableSsl = smtpSettings.GetValue("EnableSsl", true);

        string? userName = smtpSettings.GetValue<string>("UserName");
        string? password = smtpSettings.GetValue<string>("Password");

        if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
        {
            client.Credentials = new NetworkCredential(userName, password);
        }

        using var mailMessage = new MailMessage();

        string? fromAddress = _configuration.GetValue<string>(ConfigurationKeys.FromEmailAddress);

        if (string.IsNullOrWhiteSpace(fromAddress))
        {
            throw new InvalidOperationException("From address has not been configured.");
        }

        mailMessage.From = new MailAddress(fromAddress);
        mailMessage.Subject = subject;
        mailMessage.Body = htmlMessage;
        mailMessage.IsBodyHtml = true;

        mailMessage.To.Add(email);

        await client.SendMailAsync(mailMessage);
    }
}
