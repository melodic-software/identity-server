using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Admin.ApiScopes;

public class IndexModel : PageModel
{
    private readonly ApiScopeRepository _repository;

    public IndexModel(ApiScopeRepository repository)
    {
        _repository = repository;
    }

    [BindProperty(SupportsGet = true)]
    public string? Filter { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    public int TotalPages { get; set; }

    public IEnumerable<ApiScopeSummaryModel> Scopes { get; private set; } = new List<ApiScopeSummaryModel>();

    public async Task OnGetAsync()
    {
        (IEnumerable<ApiScopeSummaryModel> scopes, int totalCount) = await _repository.GetAllAsync(Filter, PageNumber, PageSize);
        Scopes = scopes;
        TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
    }
}