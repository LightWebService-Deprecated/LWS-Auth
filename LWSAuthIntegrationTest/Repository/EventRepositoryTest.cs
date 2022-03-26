using System;
using System.Linq;
using System.Threading.Tasks;
using LWSAuthService.Repository;
using LWSEvent.Event.Account;
using MassTransit.Testing;
using Xunit;

namespace LWSAuthIntegrationTest.Repository;

public class EventRepositoryTest : IDisposable
{
    private readonly BusTestHarness _testHarness;
    private readonly IEventRepository _eventRepository;

    public EventRepositoryTest()
    {
        _testHarness = new InMemoryTestHarness();

        _testHarness.Start().Wait();
        _eventRepository = new EventRepository(_testHarness.Bus);
    }

    [Fact(DisplayName =
        "PublishAccountCreated: PublishAccountCreated should publish accountCreated message to observer.")]
    public async Task Is_PublishAccountCreated_Publish_AccountCreated_Well()
    {
        // Let
        var message = new AccountCreatedEvent
        {
            AccountId = "test",
            CreatedAt = DateTimeOffset.Now
        };

        // Do
        await _eventRepository.PublishAccountCreated(message);

        // Check
        var messagePublishedResult = _testHarness.Published.Select<AccountCreatedEvent>()?.FirstOrDefault();
        Assert.NotNull(messagePublishedResult);
        var messagePublished = messagePublishedResult.Context.Message;
        Assert.NotNull(messagePublished);
        Assert.Equal(message.AccountId, messagePublished.AccountId);
        Assert.Equal(message.CreatedAt, messagePublished.CreatedAt);
    }

    [Fact(DisplayName =
        "PublishAccountDeleted: PublishAccountDeleted should publish accountDeleted message to observer.")]
    public async Task Is_PublishAccountDeleted_Publish_AccountDeleted_Well()
    {
        // Let
        var message = new AccountDeletedEvent
        {
            AccountId = "test",
            DeletedAt = DateTimeOffset.Now
        };

        // Do
        await _eventRepository.PublishAccountDeleted(message);

        // Check
        var messagePublishedResult = _testHarness.Published.Select<AccountDeletedEvent>()?.FirstOrDefault();
        Assert.NotNull(messagePublishedResult);
        var messagePublished = messagePublishedResult.Context.Message;
        Assert.NotNull(messagePublished);
        Assert.Equal(message.AccountId, messagePublished.AccountId);
        Assert.Equal(message.DeletedAt, messagePublished.DeletedAt);
    }

    public void Dispose()
    {
        _testHarness.Stop().Wait();
        _testHarness.Dispose();
    }
}