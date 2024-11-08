using Enterprise.ApplicationServices.Core.Queries.Model.Generic;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.GetLoggedInUser;

public class GetLoggedInUserQuery : IQuery<Result<User>>;