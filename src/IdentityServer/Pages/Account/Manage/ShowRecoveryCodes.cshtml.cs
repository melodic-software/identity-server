using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Account.Manage;

public class ShowRecoveryCodesModel : PageModel
{
    [TempData]
    public string[] RecoveryCodes { get; set; } = [];

    [TempData]
    public string StatusMessage { get; set; }

    public IActionResult OnGet()
    {
        if (RecoveryCodes.Length == 0)
        {
            return RedirectToPage(AccountManagementPageConstants.ManageTwoFactorAuthentication);
        }

        return Page();
    }
}