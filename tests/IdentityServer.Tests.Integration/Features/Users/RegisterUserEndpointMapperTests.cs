using Enterprise.Patterns.ResultPattern.Model.Generic;
using FluentAssertions;
using IdentityServer.AspNetIdentity.EntityFramework.DbContexts;
using IdentityServer.AspNetIdentity.Models;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.RegisterUser;
using IdentityServer.Tests.Integration.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Tests.Integration.Features.Users;

public class RegisterUserEndpointMapperTests : IClassFixture<IdentityServerFixture>
{
    private readonly IdentityServerFixture _fixture;

    public RegisterUserEndpointMapperTests(IdentityServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task RegisterUser_ShouldRegisterUser_WhenProvidedValidData()
    {
        await _fixture.ExecuteTestAsync<AspNetIdentityDbContext>(async dbContext =>
        {
            var command = new RegisterUserCommand(
                null,
                "test@test.com",
                "Pass123$",
                "John",
                "Smith",
                false,
                null,
                null,
                null
            );

            Result<string> result = await _fixture.DispatchCommandAsync(command);
            string? userId = result.Succeeded ? result.Value : null;

            result.Succeeded.Should().BeTrue();
            userId.Should().NotBeNullOrWhiteSpace();

            dbContext.ChangeTracker.Clear();

            ApplicationUser? dbUser = await dbContext.Users.FindAsync(userId);
            List<IdentityUserClaim<string>> userClaims = await dbContext.UserClaims.Where(x => x.UserId == userId).ToListAsync();
            IdentityUserClaim<string>? firstNameClaim = userClaims.Find(x => x.ClaimValue == command.FirstName);
            IdentityUserClaim<string>? lastNameClaim = userClaims.Find(x => x.ClaimValue == command.LastName);

            dbUser.Should().NotBeNull();
            dbUser!.Id.Should().BeEquivalentTo(userId);
            dbUser.Email.Should().BeEquivalentTo(command.Email);

            userClaims.Should().NotBeNull();
            firstNameClaim.Should().NotBeNull();
            lastNameClaim.Should().NotBeNull();
        });
    }
}
