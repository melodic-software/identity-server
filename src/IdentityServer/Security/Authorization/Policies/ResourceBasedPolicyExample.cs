using Microsoft.AspNetCore.Authorization;

namespace IdentityServer.Security.Authorization.Policies;

// TODO: This is an example resource based authorization policy.
// NOTE: Any authorization handlers must be registered with the DI container as a singleton.
// https://app.pluralsight.com/ilx/video-courses/77f2d072-e8ab-4806-9310-dcc770bc1ce0/25249d6d-40d4-4f01-a39a-c0d48ef17057/e2f8e4a8-eca9-4e38-8349-bd8858db4620

public class MyRequirement : IAuthorizationRequirement;
public class MyModel;

public class ResourceBasedPolicyExample : AuthorizationHandler<MyRequirement, MyModel>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MyRequirement requirement, MyModel resource)
    {
        // We can handle authorization based on the current logged-in user AND state on the resource model.
        return Task.CompletedTask;
    }
}