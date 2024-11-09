using Duende.IdentityServer.Events;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Validation;
using IdentityModel;
using IdentityServer.Logging;
using IdentityServer.Observability.Diagnostics;
using IdentityServer.Security.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Consent;

public class Index : PageModel
{
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IEventService _events;
    private readonly ILogger<Index> _logger;

    public Index(
        IIdentityServerInteractionService interaction,
        IEventService events,
        ILogger<Index> logger)
    {
        _interaction = interaction;
        _events = events;
        _logger = logger;
    }

    public ViewModel View { get; set; } = default!;

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public async Task<IActionResult> OnGet(string? returnUrl)
    {
        if (!await SetViewModelAsync(returnUrl))
        {
            return RedirectToPage(PageConstants.Error);
        }

        Input = new InputModel
        {
            ReturnUrl = returnUrl
        };

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        // Ensure the return url is still valid.
        AuthorizationRequest? authorizationRequest = await _interaction.GetAuthorizationContextAsync(Input.ReturnUrl);

        if (authorizationRequest == null)
        {
            return RedirectToPage(PageConstants.Error);
        }

        ConsentResponse? grantedConsent = null;

        // The user clicked 'no'.
        // Send back the standard 'access_denied' response.
        if (Input.Button == "no")
        {
            grantedConsent = new ConsentResponse { Error = AuthorizationError.AccessDenied };
            await _events.RaiseAsync(new ConsentDeniedEvent(User.GetSubjectId(), authorizationRequest.Client.ClientId, authorizationRequest.ValidatedResources.RawScopeValues));
            Telemetry.Metrics.ConsentDenied(authorizationRequest.Client.ClientId, authorizationRequest.ValidatedResources.ParsedScopes.Select(s => s.ParsedName));
        }
        // The user clicked 'yes'.
        // Validate the data.
        else if (Input.Button == "yes")
        {
            // If the user consented to some scope, build the response model.
            if (Input.ScopesConsented.Any())
            {
                IEnumerable<string> scopes = Input.ScopesConsented;
                if (!ConsentOptions.EnableOfflineAccess)
                {
                    scopes = scopes.Where(x => x != Duende.IdentityServer.IdentityServerConstants.StandardScopes.OfflineAccess);
                }

                grantedConsent = new ConsentResponse
                {
                    RememberConsent = Input.RememberConsent,
                    ScopesValuesConsented = scopes.ToArray(),
                    Description = Input.Description
                };

                await _events.RaiseAsync(new ConsentGrantedEvent(User.GetSubjectId(), authorizationRequest.Client.ClientId, authorizationRequest.ValidatedResources.RawScopeValues, grantedConsent.ScopesValuesConsented, grantedConsent.RememberConsent));
                Telemetry.Metrics.ConsentGranted(authorizationRequest.Client.ClientId, grantedConsent.ScopesValuesConsented, grantedConsent.RememberConsent);
                IEnumerable<string> denied = authorizationRequest.ValidatedResources.ParsedScopes.Select(s => s.ParsedName).Except(grantedConsent.ScopesValuesConsented);
                Telemetry.Metrics.ConsentDenied(authorizationRequest.Client.ClientId, denied);
            }
            else
            {
                ModelState.AddModelError(string.Empty, ConsentOptions.MustChooseOneErrorMessage);
            }
        }
        else
        {
            ModelState.AddModelError(string.Empty, ConsentOptions.InvalidSelectionErrorMessage);
        }

        if (grantedConsent != null)
        {
            ArgumentNullException.ThrowIfNull(Input.ReturnUrl);

            // Communicate outcome of consent back to IdentityServer.
            await _interaction.GrantConsentAsync(authorizationRequest, grantedConsent);

            // Redirect back to authorization endpoint.
            this.Redirect(authorizationRequest, Input.ReturnUrl, Redirect);
        }

        // We need to redisplay the consent UI.
        if (!await SetViewModelAsync(Input.ReturnUrl))
        {
            return RedirectToPage(PageConstants.Error);
        }

        return Page();
    }

    private async Task<bool> SetViewModelAsync(string? returnUrl)
    {
        ArgumentNullException.ThrowIfNull(returnUrl);

        // https://docs.duendesoftware.com/identityserver/v7/ui/consent/
        AuthorizationRequest? authorizationRequest = await _interaction.GetAuthorizationContextAsync(returnUrl);

        if (authorizationRequest != null)
        {
            View = CreateConsentViewModel(authorizationRequest);
            return true;
        }

        _logger.NoConsentMatchingRequest(returnUrl);

        return false;
    }

    private ViewModel CreateConsentViewModel(AuthorizationRequest authorizationRequest)
    {
        var vm = new ViewModel
        {
            ClientName = authorizationRequest.Client.ClientName ?? authorizationRequest.Client.ClientId,
            ClientUrl = authorizationRequest.Client.ClientUri,
            ClientLogoUrl = authorizationRequest.Client.LogoUri,
            AllowRememberConsent = authorizationRequest.Client.AllowRememberConsent,
            IdentityScopes = authorizationRequest.ValidatedResources.Resources.IdentityResources
                .Select(x => CreateScopeViewModel(x, Input.ScopesConsented.Contains(x.Name)))
                .ToArray()
        };

        IEnumerable<string> resourceIndicators = authorizationRequest.Parameters.GetValues(OidcConstants.AuthorizeRequest.Resource) ?? Enumerable.Empty<string>();
        IEnumerable<ApiResource> apiResources = authorizationRequest.ValidatedResources.Resources.ApiResources.Where(x => resourceIndicators.Contains(x.Name)).ToList();

        var apiScopes = new List<ScopeViewModel>();
        foreach (ParsedScopeValue? parsedScope in authorizationRequest.ValidatedResources.ParsedScopes)
        {
            ApiScope? apiScope = authorizationRequest.ValidatedResources.Resources.FindApiScope(parsedScope.ParsedName);

            if (apiScope == null)
            {
                continue;
            }

            ScopeViewModel scopeVm = CreateScopeViewModel(parsedScope, apiScope, Input == null || Input.ScopesConsented.Contains(parsedScope.RawValue));
            scopeVm.Resources = apiResources.Where(x => x.Scopes.Contains(parsedScope.ParsedName))
                .Select(x => new ResourceViewModel
                {
                    Name = x.Name,
                    DisplayName = x.DisplayName ?? x.Name,
                }).ToArray();

            apiScopes.Add(scopeVm);
        }

        if (ConsentOptions.EnableOfflineAccess && authorizationRequest.ValidatedResources.Resources.OfflineAccess)
        {
            apiScopes.Add(CreateOfflineAccessScope(Input == null || Input.ScopesConsented.Contains(Duende.IdentityServer.IdentityServerConstants.StandardScopes.OfflineAccess)));
        }

        vm.ApiScopes = apiScopes;

        return vm;
    }

    private static ScopeViewModel CreateScopeViewModel(IdentityResource identity, bool check)
    {
        return new ScopeViewModel
        {
            Name = identity.Name,
            Value = identity.Name,
            DisplayName = identity.DisplayName ?? identity.Name,
            Description = identity.Description,
            Emphasize = identity.Emphasize,
            Required = identity.Required,
            Checked = check || identity.Required
        };
    }

    private static ScopeViewModel CreateScopeViewModel(ParsedScopeValue parsedScopeValue, ApiScope apiScope, bool check)
    {
		string displayName = apiScope.DisplayName ?? apiScope.Name;
        if (!string.IsNullOrWhiteSpace(parsedScopeValue.ParsedParameter))
        {
            displayName += ":" + parsedScopeValue.ParsedParameter;
        }

        return new ScopeViewModel
        {
            Name = parsedScopeValue.ParsedName,
            Value = parsedScopeValue.RawValue,
            DisplayName = displayName,
            Description = apiScope.Description,
            Emphasize = apiScope.Emphasize,
            Required = apiScope.Required,
            Checked = check || apiScope.Required
        };
    }

    private static ScopeViewModel CreateOfflineAccessScope(bool check)
    {
        return new ScopeViewModel
        {
            Value = Duende.IdentityServer.IdentityServerConstants.StandardScopes.OfflineAccess,
            DisplayName = ConsentOptions.OfflineAccessDisplayName,
            Description = ConsentOptions.OfflineAccessDescription,
            Emphasize = true,
            Checked = check
        };
    }
}
