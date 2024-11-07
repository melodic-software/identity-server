using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Admin.ApiScopes;

public class NewModel : PageModel
{
    private readonly ApiScopeRepository _repository;

    public NewModel(ApiScopeRepository repository)
    {
        _repository = repository;
    }

    [BindProperty]
    public ApiScopeModel Input { get; set; } = default!;
        
    public void OnGet()
    {
        Input.IsEnabled = true;
        Input.ShowInDiscoveryDocument = true;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _repository.CreateAsync(Input);

        return RedirectToPage(AdminPageConstants.ClientsEdit, new { id = Input.Name });
    }
}
