using System.Text.Json;
using Confluent.Kafka;
using InsuranceUnderwriting.Contracts;
using InsuranceUnderwriting.Write.Application;

namespace InsuranceUnderwriting.Write.Infrastructure.Kafka;

public class KafkaIntegrationEventPublisher : IIntegrationEventPublisher, IAsyncDisposable
{
    private readonly IProducer<string, string> _producer;

    public KafkaIntegrationEventPublisher(string bootstrapServers)
    {
        _producer = new ProducerBuilder<string, string>(new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        }).Build();
    }

    public async Task PublishAsync(Guid applicationId, object domainEvent)
    {
        var (eventType, payload) = IntegrationEventMapper.ToIntegrationEvent(domainEvent);
        var message = new IntegrationEventMessage(eventType, JsonSerializer.Serialize(payload, payload.GetType()));

        // Key = applicationId keeps every event for one application in the same
        // partition, so the Read service's consumer sees them in order.
        await _producer.ProduceAsync(KafkaTopics.ApplicationEvents, new Message<string, string>
        {
            Key = applicationId.ToString(),
            Value = JsonSerializer.Serialize(message)
        });
    }

    public ValueTask DisposeAsync()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
        return ValueTask.CompletedTask;
    }
}
