using LWSEvent.Event.Account;
using MassTransit;

namespace LWSAuthService.Repository;

public interface IEventRepository
{
    Task PublishAccountCreated(AccountCreatedEvent accountCreatedMessage);
    Task PublishAccountDeleted(AccountDeletedEvent accountDeletedMessage);
}

public class EventRepository : IEventRepository
{
    private readonly IPublishEndpoint _publishEndpoint;

    public EventRepository(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishAccountCreated(AccountCreatedEvent accountCreatedMessage)
    {
        await _publishEndpoint.Publish<AccountCreatedEvent>(accountCreatedMessage);
    }

    public async Task PublishAccountDeleted(AccountDeletedEvent accountDeletedMessage)
    {
        await _publishEndpoint.Publish<AccountDeletedEvent>(accountDeletedMessage);
    }
}