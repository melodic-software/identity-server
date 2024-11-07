using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Admin.ApiScopes;

public class EditModel : PageModel
{
    private readonly ApiScopeRepository _repository;

    public EditModel(ApiScopeRepository repository)
    {
        _repository = repository;
    }

    [BindProperty]
    public ApiScopeModel Input { get; set; } = default!;

    [BindProperty]
    public string Button { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(string id)
    {
        ApiScopeModel? model = await _repository.GetByIdAsync(id);

        if (model == null)
        {
            return RedirectToPage(AdminPageConstants.ApiScopes);
        }

        Input = model;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string id)
    {
        if (Button == "delete")
        {
            await _repository.DeleteAsync(id);
            return RedirectToPage(AdminPageConstants.ApiScopes);
        }

        if (ModelState.IsValid)
        {
            await _repository.UpdateAsync(Input);
            return RedirectToPage(AdminPageConstants.ApiScopes);
        }

        return Page();
    }
}
