using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Pages.Admin.ApiScopes.Properties;

public class AddModel : PageModel
{
    private readonly ConfigurationDbContext _dbContext;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public AddModel(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> OnGetAsync(int apiScopeId)
    {
        ApiScope? apiScope = await _dbContext.FindAsync<ApiScope>(apiScopeId);

        if (apiScope == null)
        {
            return RedirectToPage(AdminPageConstants.ApiScopes);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int apiScopeId)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        ApiScope? apiScope = await _dbContext.FindAsync<ApiScope>(apiScopeId);

        if (apiScope == null)
        {
            return RedirectToPage(AdminPageConstants.ApiScopes);
        }

        var property = new ApiScopeProperty
        {
            Key = Input.Key,
            Value = Input.Value
        };

        apiScope.Properties.Add(property);

        await _dbContext.SaveChangesAsync();

        return RedirectToPage(AdminPageConstants.ApiScopes);
    }

    public class InputModel
    {
        [Required]
        public string Key { get; set; }

        [Required]
        public string Value { get; set; }
    }
}