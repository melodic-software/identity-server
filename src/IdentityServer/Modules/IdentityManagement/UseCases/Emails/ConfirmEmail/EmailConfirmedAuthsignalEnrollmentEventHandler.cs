using Authsignal;
using Enterprise.Events.Handlers.Abstract.Generic.Base;
using IdentityServer.Constants;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Emails.ConfirmEmail;

public sealed class EmailConfirmedAuthsignalEnrollmentEventHandler : EventHandlerBase<EmailConfirmedDomainEvent>
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly IAuthsignalClient _authsignalClient;
    private readonly ILogger<EmailConfirmedAuthsignalEnrollmentEventHandler> _logger;

    public EmailConfirmedAuthsignalEnrollmentEventHandler(
        IConfiguration configuration,
        IHostEnvironment environment,
        IAuthsignalClient authsignalClient,
        ILogger<EmailConfirmedAuthsignalEnrollmentEventHandler> logger)
    {
        _configuration = configuration;
        _environment = environment;
        _authsignalClient = authsignalClient;
        _logger = logger;
    }

    public override async Task HandleAsync(EmailConfirmedDomainEvent @event, CancellationToken cancellationToken = default)
    {
        if (!_configuration.GetValue(ConfigurationKeys.AuthsignalEnabled, false))
        {
            return;
        }

        string email = @event.Email;

        if (!_environment.IsProduction())
        {
            // We don't want these auth challenges going out to users in pre-production environments.
            string? preProdEmailRecipient = _configuration.GetValue<string>(ConfigurationKeys.PreProdEmailRecipient);

            if (string.IsNullOrWhiteSpace(preProdEmailRecipient))
            {
                _logger.LogError(
                    "The pre-production email recipient has not been configured. " +
                    "Default authenticators will not be registered for user with ID: {UserId}",
                    @event.UserId
                );

                throw new InvalidOperationException();
            }

            email = preProdEmailRecipient;
        }

        // We can auto enroll the user with these email based authenticators.
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
