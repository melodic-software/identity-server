using Enterprise.ApplicationServices.Core.Queries.Handlers.Bound.Logic;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Passwords.GetPasswordRequirements;

public class GetPasswordRequirementsQueryLogic : IQueryLogic<GetPasswordRequirementsQuery, PasswordRequirements>
{
    private readonly PasswordOptions _passwordOptions;

    public GetPasswordRequirementsQueryLogic(IOptions<PasswordOptions> passwordOptions, IOptions<IdentityOptions> identityOptions)
    {
        _passwordOptions = passwordOptions.Value;
    }

    public Task<PasswordRequirements> ExecuteAsync(GetPasswordRequirementsQuery query, CancellationToken cancellationToken = default)
    {
        var passwordRequirements = new PasswordRequirements(
            _passwordOptions.RequiredLength,
            _passwordOptions.RequiredUniqueChars,
            _passwordOptions.RequireNonAlphanumeric,
            _passwordOptions.RequireLowercase,
            _passwordOptions.RequireUppercase,
            _passwordOptions.RequireDigit
        );

        return Task.FromResult(passwordRequirements);
    }
}