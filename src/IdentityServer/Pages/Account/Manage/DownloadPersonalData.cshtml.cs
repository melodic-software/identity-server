using IdentityServer.AspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection;
using System.Text.Json;

namespace IdentityServer.Pages.Account.Manage;

public class DownloadPersonalDataModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DownloadPersonalDataModel> _logger;

    public DownloadPersonalDataModel(
        UserManager<ApplicationUser> userManager,
        ILogger<DownloadPersonalDataModel> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public IActionResult OnGet()
    {
        return NotFound();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ApplicationUser user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        _logger.LogInformation("User with ID '{UserId}' asked for their personal data.", _userManager.GetUserId(User));

        // Only include personal data for download
        var personalData = new Dictionary<string, string>();
        IEnumerable<PropertyInfo> personalDataProps = typeof(ApplicationUser).GetProperties()
            .Where(prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));

        foreach (PropertyInfo p in personalDataProps)
        {
            personalData.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
        }

        IList<UserLoginInfo> logins = await _userManager.GetLoginsAsync(user);

        foreach (UserLoginInfo l in logins)
        {
            personalData.Add($"{l.LoginProvider} external login provider key", l.ProviderKey);
        }

        string? authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);

        if (!string.IsNullOrWhiteSpace(authenticatorKey))
        {
            personalData.Add($"Authenticator Key", authenticatorKey);
        }

        Response.Headers.TryAdd("Content-Disposition", "attachment; filename=PersonalData.json");

        byte[] fileContents = JsonSerializer.SerializeToUtf8Bytes(personalData);

        return new FileContentResult(fileContents, "application/json");
    }
}