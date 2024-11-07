using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using System.Text;
using System.Text.Json;

namespace IdentityServer.Pages.Diagnostics;

public class ViewModel
{
    public ViewModel(AuthenticateResult result)
    {
        AuthenticateResult = result;

        if (result?.Properties?.Items.TryGetValue("client_list", out string? encoded) == true && encoded != null)
        {
            byte[] bytes = Base64Url.Decode(encoded);
            string value = Encoding.UTF8.GetString(bytes);
            Clients = JsonSerializer.Deserialize<string[]>(value) ?? Enumerable.Empty<string>();
            return;
        }
        Clients = [];
    }

    public AuthenticateResult AuthenticateResult { get; }
    public IEnumerable<string> Clients { get; }
}
