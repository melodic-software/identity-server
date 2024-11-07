using Duende.IdentityServer.Configuration;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Validation;
using IdentityServer.Diagnostics;
using IdentityServer.Pages.Consent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace IdentityServer.Pages.Device;

public class Index : PageModel
{
    private readonly IDeviceFlowInteractionService _interaction;
    private readonly IEventService _events;
    private readonly IOptions<IdentityServerOptions> _options;
    private readonly ILogger<Index> _logger;

    public Index(
        IDeviceFlowInteractionService interaction,
        IEventService eventService,
        IOptions<IdentityServerOptions> options,
        ILogger<Index> logger)
    {
        _interaction = interaction;
        _events = eventService;
        _options = options;
        _logger = logger;
    }

    public ViewModel View { get; set; } = default!;

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public async Task<IActionResult> OnGet(string? userCode)
    {
        if (string.IsNullOrWhiteSpace(userCode))
        {
            return Page();
        }

        if (!await SetViewModelAsync(userCode))
        {
            ModelState.AddModelError(string.Empty, DeviceOptions.InvalidUserCode);
            return Page();
        }

        Input = new InputModel { 
            UserCode = userCode,
        };

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Input.UserCode);
        DeviceFlowAuthorizationRequest? authorizationRequest = await _interaction.GetAuthorizationContextAsync(Input.UserCode);

        if (authorizationRequest == null)
        {
            return RedirectToPage(PageConstants.Error);
        }

        ConsentResponse? grantedConsent = null;

        // The user clicked 'no'.
        // Send back the standard 'access_denied' response.
        if (Input.Button == "no")
        {
            grantedConsent = new ConsentResponse
            {
                Error = AuthorizationError.AccessDenied
            };

            // emit event
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
            // Communicate outcome of consent back to IdentityServer.
            await _interaction.HandleRequestAsync(Input.UserCode, grantedConsent);

            // Indicate that's it ok to redirect back to authorization endpoint.
            return RedirectToPage(PageConstants.DeviceVerificationSuccess);
        }

        // We need to redisplay the consent UI.
        if (!await SetViewModelAsync(Input.UserCode))
        {
            return RedirectToPage(PageConstants.Error);
        }
        return Page();
    }


    private async Task<bool> SetViewModelAsync(string userCode)
    {
        DeviceFlowAuthorizationRequest? authorizationRequest = await _interaction.GetAuthorizationContextAsync(userCode);

        if (authorizationRequest != null)
        {
            View = CreateConsentViewModel(authorizationRequest);
            return true;
        }

        View = new ViewModel();
        return false;
    }

    private ViewModel CreateConsentViewModel(DeviceFlowAuthorizationRequest authorizationRequest)
    {
        var vm = new ViewModel
        {
            ClientName = authorizationRequest.Client.ClientName ?? authorizationRequest.Client.ClientId,
            ClientUrl = authorizationRequest.Client.ClientUri,
            ClientLogoUrl = authorizationRequest.Client.LogoUri,
            AllowRememberConsent = authorizationRequest.Client.AllowRememberConsent,
            IdentityScopes = authorizationRequest.ValidatedResources.Resources.IdentityResources
                .Select(x => CreateScopeViewModel(x, Input.ScopesConsented.Contains(x.Name))).ToArray()
        };

        var apiScopes = new List<ScopeViewModel>();
        foreach (ParsedScopeValue? parsedScope in authorizationRequest.ValidatedResources.ParsedScopes)
        {
            ApiScope? apiScope = authorizationRequest.ValidatedResources.Resources.FindApiScope(parsedScope.ParsedName);
            
            if (apiScope == null)
            {
                continue;
            }

            ScopeViewModel scopeVm = CreateScopeViewModel(parsedScope, apiScope, Input == null || Input.ScopesConsented.Contains(parsedScope.RawValue));
            
            apiScopes.Add(scopeVm);
        }

        if (DeviceOptions.EnableOfflineAccess && authorizationRequest.ValidatedResources.Resources.OfflineAccess)
        {
            apiScopes.Add(GetOfflineAccessScope(Input == null || Input.ScopesConsented.Contains(Duende.IdentityServer.IdentityServerConstants.StandardScopes.OfflineAccess)));
        }

        vm.ApiScopes = apiScopes;

        return vm;
    }

    private static ScopeViewModel CreateScopeViewModel(IdentityResource identity, bool check)
    {
        return new ScopeViewModel
        {
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
        return new ScopeViewModel
        {
            Value = parsedScopeValue.RawValue,
            // TODO: Use the parsed scope value in the display?
            DisplayName = apiScope.DisplayName ?? apiScope.Name,
            Description = apiScope.Description,
            Emphasize = apiScope.Emphasize,
            Required = apiScope.Required,
            Checked = check || apiScope.Required
        };
    }

    private static ScopeViewModel GetOfflineAccessScope(bool check)
    {
        return new ScopeViewModel
        {
            Value = Duende.IdentityServer.IdentityServerConstants.StandardScopes.OfflineAccess,
            DisplayName = DeviceOptions.OfflineAccessDisplayName,
            Description = DeviceOptions.OfflineAccessDescription,
            Emphasize = true,
            Checked = check
        };
    }
}
