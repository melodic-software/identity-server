using Duende.IdentityServer.Events;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Validation;
using IdentityServer.Logging;
using IdentityServer.Observability.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Ciba;

public class Consent : PageModel
{
    private readonly IBackchannelAuthenticationInteractionService _interaction;
    private readonly IEventService _events;
    private readonly ILogger<Consent> _logger;

    public Consent(
        IBackchannelAuthenticationInteractionService interaction,
        IEventService events,
        ILogger<Consent> logger)
    {
        _interaction = interaction;
        _events = events;
        _logger = logger;
    }

    public ViewModel View { get; set; } = default!;

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public async Task<IActionResult> OnGet(string? id)
    {
        if (!await SetViewModelAsync(id))
        {
            return RedirectToPage(PageConstants.Error);
        }

        Input = new InputModel
        {
            Id = id
        };

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        // validate return url is still valid
        ArgumentException.ThrowIfNullOrWhiteSpace(Input.Id);

        BackchannelUserLoginRequest? request = await _interaction.GetLoginRequestByInternalIdAsync(id: Input.Id);
        
        if (request == null || request.Subject.GetSubjectId() != User.GetSubjectId())
        {
            _logger.InvalidId(Input.Id);
            return RedirectToPage(PageConstants.Error);
        }

        CompleteBackchannelLoginRequest? result = null;

        // User clicked 'no'.
        // Send back the standard 'access_denied' response.
        if (Input.Button == "no")
        {
            result = new CompleteBackchannelLoginRequest(Input.Id);

            // emit event
            await _events.RaiseAsync(new ConsentDeniedEvent(User.GetSubjectId(), request.Client.ClientId, request.ValidatedResources.RawScopeValues));
            Telemetry.Metrics.ConsentDenied(request.Client.ClientId, request.ValidatedResources.ParsedScopes.Select(s => s.ParsedName));
        }
        // User clicked 'yes'.
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

                result = new CompleteBackchannelLoginRequest(Input.Id)
                {
                    ScopesValuesConsented = scopes.ToArray(),
                    Description = Input.Description
                };

                await _events.RaiseAsync(new ConsentGrantedEvent(User.GetSubjectId(), request.Client.ClientId, request.ValidatedResources.RawScopeValues, result.ScopesValuesConsented, false));
                Telemetry.Metrics.ConsentGranted(request.Client.ClientId, result.ScopesValuesConsented, false);
                IEnumerable<string> denied = request.ValidatedResources.ParsedScopes.Select(s => s.ParsedName).Except(result.ScopesValuesConsented);
                Telemetry.Metrics.ConsentDenied(request.Client.ClientId, denied);
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

        if (result != null)
        {
            // Communicate outcome of consent back to IdentityServer.
            await _interaction.CompleteLoginRequestAsync(result);

            return RedirectToPage(PageConstants.CibaAll);
        }

        // We need to redisplay the consent UI.
        if (!await SetViewModelAsync(Input.Id))
        {
            return RedirectToPage(PageConstants.Error);
        }

        return Page();
    }

    private async Task<bool> SetViewModelAsync(string? id)
    {
        ArgumentNullException.ThrowIfNull(id);

        BackchannelUserLoginRequest? request = await _interaction.GetLoginRequestByInternalIdAsync(id);

        if (request != null && request.Subject.GetSubjectId() == User.GetSubjectId())
        {
            View = CreateConsentViewModel(request);
            return true;
        }

        _logger.NoMatchingBackchannelLoginRequest(id);

        return false;
    }

    private ViewModel CreateConsentViewModel(BackchannelUserLoginRequest request)
    {
        var vm = new ViewModel
        {
            ClientName = request.Client.ClientName ?? request.Client.ClientId,
            ClientUrl = request.Client.ClientUri,
            ClientLogoUrl = request.Client.LogoUri,
            BindingMessage = request.BindingMessage,
            IdentityScopes = request.ValidatedResources.Resources.IdentityResources
                .Select(x => CreateScopeViewModel(x, Input.ScopesConsented.Contains(x.Name)))
                .ToArray()
        };

        IEnumerable<string> resourceIndicators = request.RequestedResourceIndicators ?? Enumerable.Empty<string>();

        IEnumerable<ApiResource> apiResources = request.ValidatedResources.Resources.ApiResources
            .Where(x => resourceIndicators.Contains(x.Name)).ToList();

        var apiScopes = new List<ScopeViewModel>();

        foreach (ParsedScopeValue? parsedScope in request.ValidatedResources.ParsedScopes)
        {
            ApiScope? apiScope = request.ValidatedResources.Resources.FindApiScope(parsedScope.ParsedName);

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

        if (ConsentOptions.EnableOfflineAccess && request.ValidatedResources.Resources.OfflineAccess)
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

    private static ScopeViewModel GetOfflineAccessScope(bool check)
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
