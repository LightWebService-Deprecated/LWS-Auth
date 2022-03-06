using Confluent.Kafka;
using Newtonsoft.Json;

namespace LWSAuthService.Repository;

public interface IEventRepository
{
    Task SendMessageToTopicAsync(string topic, object message);
}

public class EventRepository : IDisposable, IEventRepository
{
    private readonly IProducer<Null, string> _messageProducer;

    public EventRepository(ProducerConfig producerConfig)
    {
        _messageProducer = new ProducerBuilder<Null, string>(producerConfig).Build();
    }

    public async Task SendMessageToTopicAsync(string topic, object message)
    {
        await _messageProducer.ProduceAsync(topic,
            new Message<Null, string> {Value = JsonConvert.SerializeObject(message)});
    }

    public void Dispose()
    {
        _messageProducer.Dispose();
    }
}