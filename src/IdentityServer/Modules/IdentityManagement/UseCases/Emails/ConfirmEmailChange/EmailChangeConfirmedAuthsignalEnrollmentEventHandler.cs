using Authsignal;
using Enterprise.Events.Handlers.Abstract.Generic.Base;
using IdentityServer.Constants;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Emails.ConfirmEmailChange;

public class EmailChangeConfirmedAuthsignalEnrollmentEventHandler : EventHandlerBase<EmailChangeConfirmedDomainEvent>
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly IAuthsignalClient _authsignalClient;

    public EmailChangeConfirmedAuthsignalEnrollmentEventHandler(
        IConfiguration configuration,
        IHostEnvironment environment,
        IAuthsignalClient authsignalClient)
    {
        _configuration = configuration;
        _environment = environment;
        _authsignalClient = authsignalClient;
    }

    public override async Task HandleAsync(EmailChangeConfirmedDomainEvent @event, CancellationToken cancellationToken = default)
    {
        if (!_configuration.GetValue(ConfigurationKeys.AuthsignalEnabled, false))
        {
            return;
        }

        string email = @event.NewEmail;

        if (!_environment.IsProduction())
        {
            // We don't want these auth challenges going out to users in pre-production environments.
            string? preProdEmailRecipient = _configuration.GetValue<string>(ConfigurationKeys.PreProdEmailRecipient);

            if (string.IsNullOrWhiteSpace(preProdEmailRecipient))
            {
                throw new InvalidOperationException("The pre-production email recipient has not been configured.");
            }

            email = preProdEmailRecipient;
        }

        // We can update and auto enroll the user with these email based authenticators.
        // TODO: Do we want a feature toggle on this?
        await EnrollEmailAuthenticatorAsync(@event.UserId, email, VerificationMethod.EMAIL_OTP, cancellationToken);
        await EnrollEmailAuthenticatorAsync(@event.UserId, email, VerificationMethod.EMAIL_MAGIC_LINK, cancellationToken);
    }

    // TODO: This method is duplicated. Let's move this and update the areas where it is used.
    private async Task EnrollEmailAuthenticatorAsync(string userId, string email, VerificationMethod verificationMethod, CancellationToken cancellationToken)
    {
        var request = new EnrollVerifiedAuthenticatorRequest(userId, verificationMethod, null, email);
        await _authsignalClient.EnrollVerifiedAuthenticator(request, cancellationToken);
    }
}