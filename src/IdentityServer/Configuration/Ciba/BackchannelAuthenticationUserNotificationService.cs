using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Enterprise.ApplicationServices.Core.Queries.Dispatching.Facade;
using Enterprise.Patterns.ResultPattern.Model.Generic;
using IdentityModel;
using IdentityServer.AspNetIdentity.Email;
using IdentityServer.Constants;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.GetUserById;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdentityServer.Configuration.Ciba;

// https://docs.duendesoftware.com/identityserver/v7/ui/ciba/
// https://docs.duendesoftware.com/identityserver/v7/reference/endpoints/ciba/
// https://www.identityserver.com/articles/ciba-in-identityserver
// https://openid.net/specs/openid-client-initiated-backchannel-authentication-core-1_0.html
// https://openid.net/wg/fapi/

public class BackchannelAuthenticationUserNotificationService : IBackchannelAuthenticationUserNotificationService
{
    private readonly IQueryDispatchFacade _queryDispatcher;
    private readonly IUrlHelper _urlHelper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly EmailService _emailService;
    private readonly ILogger<BackchannelAuthenticationUserNotificationService> _logger;

    public BackchannelAuthenticationUserNotificationService(
        IQueryDispatchFacade queryDispatcher,
        IUrlHelper urlHelper,
        IHttpContextAccessor httpContextAccessor,
        EmailService emailService,
        ILogger<BackchannelAuthenticationUserNotificationService> logger)
    {
        _queryDispatcher = queryDispatcher;
        _urlHelper = urlHelper;
        _httpContextAccessor = httpContextAccessor;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task SendLoginRequestAsync(BackchannelUserLoginRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        string? userId = request.Subject.FindFirstValue(JwtClaimTypes.Subject);

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogError("Subject claim not found in the request.");
            throw new InvalidOperationException(ErrorMessages.UnknownUserId);
        }

        var getUserByIdQuery = new GetUserByIdQuery(userId);
        Result<User> getUserByIdResult = await _queryDispatcher.DispatchAsync(getUserByIdQuery);

        if (getUserByIdResult.HasErrors)
        {
            throw new InvalidOperationException(getUserByIdResult.FirstError.Message);
        }

        User user = getUserByIdResult.Value;

        if (string.IsNullOrEmpty(user.Email))
        {
            _logger.LogError("User {UserId} does not have an email address.", userId);
            return;
        }

        HttpContext? httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            throw new InvalidOperationException("HTTP context cannot be null.");
        }

        try
        {
            await _emailService.SendCibaLoginRequestEmailAsync(_urlHelper, httpContext, user.Email, request);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending login request notification email to {Email} for request {RequestId}.", user.Email, request.InternalId);
            throw;
        }
    }
}
