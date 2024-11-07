using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Pages.Admin.ApiScopes.Properties;

public class UpdateModel : PageModel
{
    private readonly ConfigurationDbContext _dbContext;

    public UpdateModel(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [BindProperty]
    public List<InputModel> Inputs { get; set; }

    public async Task<IActionResult> OnGet(int apiScopeId)
    {
        ApiScope? apiScope = await _dbContext.ApiScopes
            .Where(x => x.Id == apiScopeId)
            .Include(x => x.Properties)
            .FirstOrDefaultAsync();

        if (apiScope == null)
        {
            return NotFound();
        }

        var properties = apiScope.Properties.ToList();

        Inputs = properties.Select(x => new InputModel { Key = x.Key, Value = x.Value }).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPost(int apiScopeId)
    {
        ApiScope? apiScope = await _dbContext.ApiScopes
            .Where(x => x.Id == apiScopeId)
            .Include(x => x.Properties)
            .FirstOrDefaultAsync();

        if (apiScope == null)
        {
            return NotFound();
        }

        var properties = apiScope.Properties
            .ToDictionary(x => x.Key, x => x.Value);

        var toAdd = Inputs.Where(x => !properties.ContainsKey(x.Key)).ToList();
        var toUpdate = Inputs.Where(x => properties.ContainsKey(x.Key)).ToList();
        var toDelete = properties.Where(p => Inputs.Any(i => i.Key == p.Key)).ToList();

        foreach (InputModel inputModel in toAdd)
        {
            apiScope.Properties.Add(new ApiScopeProperty { Key = inputModel.Key, Value = inputModel.Value});
        }

        foreach (InputModel inputModel in toUpdate)
        {
            ApiScopeProperty property = apiScope.Properties.First(x => x.Key == inputModel.Key);
            property.Value = inputModel.Value;
        }

        foreach (KeyValuePair<string, string> kvp in toDelete)
        {
            ApiScopeProperty property = apiScope.Properties.First(x => x.Key == kvp.Key);
            apiScope.Properties.Remove(property);
        }

        await _dbContext.SaveChangesAsync();

        return RedirectToPage(AdminPageConstants.ApiScopes);
    }

    public class InputModel
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}