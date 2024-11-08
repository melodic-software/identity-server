using Enterprise.Library.Core.Networking.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Diagnostics;

public class Index : PageModel
{
    public ViewModel View { get; set; } = default!;

    public async Task<IActionResult> OnGet()
    {
        var localAddresses = new List<string?>
        {
            IpAddresses.LoopbackIPv4,
            IpAddresses.LoopbackIPv6
        };

        if (HttpContext.Connection.LocalIpAddress != null)
        {
            localAddresses.Add(HttpContext.Connection.LocalIpAddress.ToString());
        }

        if (!localAddresses.Contains(HttpContext.Connection.RemoteIpAddress?.ToString()))
        {
            return NotFound();
        }

        AuthenticateResult authenticateResult = await HttpContext.AuthenticateAsync();

        View = new ViewModel(authenticateResult);
            
        return Page();
    }
}
