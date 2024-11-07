using Duende.IdentityServer;
using Duende.IdentityServer.Configuration;
using Duende.IdentityServer.ResponseHandling;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Validation;

namespace IdentityServer.Configuration.Services.UserInteraction;

// https://docs.duendesoftware.com/identityserver/v7/reference/response_handling/authorize_interaction_response_generator/
// https://docs.duendesoftware.com/identityserver/v7/ui/custom/

public class CustomAuthorizeInteractionResponseGenerator : AuthorizeInteractionResponseGenerator
{
    public CustomAuthorizeInteractionResponseGenerator(IdentityServerOptions options, IClock clock,
        ILogger<CustomAuthorizeInteractionResponseGenerator> logger, IConsentService consent, IProfileService profile)
        : base(options, clock, logger, consent, profile)
    {
    }

    protected override async Task<InteractionResponse> ProcessLoginAsync(ValidatedAuthorizeRequest request)
    {
        InteractionResponse? result = await base.ProcessLoginAsync(request);

        bool userMustLogin = result.IsLogin;
        bool userMustConsent = result.IsConsent;
        bool userMustCreateAccount = result.IsCreateAccount;

        bool isError = result.IsError;
        string? error = result.Error;
        string? errorDescription = result.ErrorDescription;

        bool isRedirect = result.IsRedirect;
        string? redirectUrl = result.RedirectUrl;

        // TODO: Add any intercepts here if needed.

        //if (result.IsLogin || result.IsError)
        //{
        //    return result;
        //}

        // check EULA database
        //var mustShowEulaPage = HasUserAcceptedEula(request.Subject);

        //if (!mustShowEulaPage)
        //{
        //    result = new InteractionResponse
        //    {
        //        RedirectUrl = "/eula/accept"
        //    };
        //}

        return result;
    }


}
