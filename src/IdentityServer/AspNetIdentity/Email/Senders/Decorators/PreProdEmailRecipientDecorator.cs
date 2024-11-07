using Enterprise.Applications.AspNetCore.Security.PII;
using Enterprise.DesignPatterns.Decorator.Instances.Abstract;
using Enterprise.DesignPatterns.Decorator.Model.Generic.Base;
using IdentityServer.Constants;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace IdentityServer.AspNetIdentity.Email.Senders.Decorators;

public class PreProdEmailRecipientDecorator : Decorator<IEmailSender>, IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<PreProdEmailRecipientDecorator> _logger;

    public PreProdEmailRecipientDecorator(
        IConfiguration configuration,
        IEmailSender decorated,
        IGetDecoratedInstance decoratorService,
        ILogger<PreProdEmailRecipientDecorator> logger)
        : base(decorated, decoratorService)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // We change the email address so they all get routed to a shared inbox that is used for testing purposes.
        string? preProdEmailRecipient = _configuration.GetValue<string>(ConfigurationKeys.PreProdEmailRecipient);

        if (string.IsNullOrWhiteSpace(preProdEmailRecipient))
        {
            throw new InvalidOperationException("The pre-production email recipient has not been configured.");
        }

        // Mask the original email address for data protection / security reasons.
        string originalMaskedEmail = EmailMasker.MaskEmail(email);

        _logger.LogInformation(
            "Updating email address to {NewEmailAddress}. " +
            "Original email address: {OriginalEmailAddress}",
            preProdEmailRecipient, originalMaskedEmail
        );

        await Decorated.SendEmailAsync(preProdEmailRecipient, subject, htmlMessage);
    }
}
