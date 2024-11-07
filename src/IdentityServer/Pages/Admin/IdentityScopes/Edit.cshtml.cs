using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Admin.IdentityScopes;

public class EditModel : PageModel
{
    private readonly IdentityScopeRepository _repository;

    public EditModel(IdentityScopeRepository repository)
    {
        _repository = repository;
    }

    [BindProperty]
    public IdentityScopeModel InputModel { get; set; } = default!;

    [BindProperty]
    public string? Button { get; set; }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        IdentityScopeModel? model = await _repository.GetByIdAsync(id);

        if (model == null)
        {
            return RedirectToPage(AdminPageConstants.IdentityScopes);
        }

        InputModel = model;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string id)
    {
        if (Button == "delete")
        {
            await _repository.DeleteAsync(id);
            return RedirectToPage(AdminPageConstants.IdentityScopes);
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _repository.UpdateAsync(InputModel);
        return RedirectToPage(AdminPageConstants.IdentityScopesEdit, new { id });

    }
}
