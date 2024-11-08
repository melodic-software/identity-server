using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Admin.IdentityScopes;

public class NewModel : PageModel
{
    private readonly IdentityScopeRepository _repository;

    public NewModel(IdentityScopeRepository repository)
    {
        _repository = repository;
    }

    [BindProperty]
    public IdentityScopeModel InputModel { get; set; } = default!;

    public void OnGet()
    {
        // Nothing here.
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            await _repository.CreateAsync(InputModel);
            return RedirectToPage(AdminPageConstants.IdentityScopesEdit, new { id = InputModel.Name });
        }

        return Page();
    }
}
