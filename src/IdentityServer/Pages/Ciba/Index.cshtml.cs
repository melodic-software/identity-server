using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityServer.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Ciba;

// https://docs.duendesoftware.com/identityserver/v7/ui/ciba/
// https://docs.duendesoftware.com/identityserver/v7/reference/endpoints/ciba/
// https://www.identityserver.com/articles/ciba-in-identityserver
// https://openid.net/specs/openid-client-initiated-backchannel-authentication-core-1_0.html
// https://openid.net/wg/fapi/

[AllowAnonymous]
public class IndexModel : PageModel
{
    public BackchannelUserLoginRequest LoginRequest { get; set; } = default!;

    private readonly IBackchannelAuthenticationInteractionService _backchannelAuthenticationInteraction;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IBackchannelAuthenticationInteractionService backchannelAuthenticationInteractionService, ILogger<IndexModel> logger)
    {
        _backchannelAuthenticationInteraction = backchannelAuthenticationInteractionService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGet(string id)
    {
        BackchannelUserLoginRequest? result = await _backchannelAuthenticationInteraction.GetLoginRequestByInternalIdAsync(id);

        if (result == null)
        {
            _logger.InvalidBackchannelLoginId(id);
            return RedirectToPage(PageConstants.Error);
        }

        LoginRequest = result;

        return Page();
    }
}
