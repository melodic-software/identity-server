using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Admin.Clients;

public class EditModel : PageModel
{
    private readonly ClientRepository _repository;

    public EditModel(ClientRepository repository)
    {
        _repository = repository;
    }

    [FromRoute]
    public string Id { get; set; }

    [BindProperty]
    public ClientModel InputModel { get; set; } = default!;

    [BindProperty]
    public string? Button { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        ClientModel? model = await _repository.GetByIdAsync(Id);

        if (model == null)
        {
            return RedirectToPage(AdminPageConstants.Clients);
        }

        InputModel = model; 
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string id)
    {
        if (Button == "delete")
        {
            await _repository.DeleteAsync(id);
            return RedirectToPage(AdminPageConstants.Clients);
        }

        if (ModelState.IsValid)
        {
            await _repository.UpdateAsync(InputModel);
            return RedirectToPage(AdminPageConstants.ClientsEdit, new { id });
        }

        return Page();
    }
}
