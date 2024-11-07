using Enterprise.ApplicationServices.Core.Queries.Dispatching.Facade;
using Enterprise.Events.Handlers.Abstract.Generic.Base;
using Enterprise.Events.Integration;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.GetUserById;
using IdentityServer.Modules.IdentityManagement.UseCases.Users.Shared;

namespace IdentityServer.Modules.IdentityManagement.UseCases.Users.RegisterUser;

public class UserRegisteredEventHandler : EventHandlerBase<UserRegisteredDomainEvent>
{
    private readonly IQueryDispatchFacade _queryDispatcher;
    private readonly IIntegrationEventBus _eventBus;

    public UserRegisteredEventHandler(IQueryDispatchFacade queryDispatcher, IIntegrationEventBus eventBus)
    {
        _queryDispatcher = queryDispatcher;
        _eventBus = eventBus;
    }

    public override async Task HandleAsync(UserRegisteredDomainEvent @event, CancellationToken cancellationToken = default)
    {
        var query = new GetUserByIdQuery(@event.UserId);

        User? queryResult = await _queryDispatcher.DispatchAsync(query, cancellationToken);

        if (queryResult == null)
        {
            throw new Exception("User query returned null. Cannot publish integration event.");
        }

        var integrationEvent = new UserRegisteredIntegrationEvent(queryResult.UserId, queryResult.Email, queryResult.FirstName, queryResult.LastName);

        await _eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
