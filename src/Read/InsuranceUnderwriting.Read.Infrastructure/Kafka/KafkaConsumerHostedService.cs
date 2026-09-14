using System.Text.Json;
using Confluent.Kafka;
using InsuranceUnderwriting.Contracts;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InsuranceUnderwriting.Read.Infrastructure.Kafka;

// Consumes integration events published by the Write service and feeds them
// into ApplicationReadModelUpdater to keep the denormalized read model current.
public class KafkaConsumerHostedService : BackgroundService
{
    private readonly string _bootstrapServers;
    private readonly string _consumerGroup;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<KafkaConsumerHostedService> _logger;

    public KafkaConsumerHostedService(
        string bootstrapServers,
        string consumerGroup,
        IServiceScopeFactory scopeFactory,
        ILogger<KafkaConsumerHostedService> logger)
    {
        _bootstrapServers = bootstrapServers;
        _consumerGroup = consumerGroup;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        => Task.Run(() => Consume(stoppingToken), stoppingToken);

    private async Task Consume(CancellationToken stoppingToken)
    {
        using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = _bootstrapServers,
            GroupId = _consumerGroup,
            AutoOffsetReset = AutoOffsetReset.Earliest
        }).Build();

        consumer.Subscribe(KafkaTopics.ApplicationEvents);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<string, string>? result;
                try
                {
                    result = consumer.Consume(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Failed to consume from {Topic}", KafkaTopics.ApplicationEvents);
                    continue;
                }

                if (result?.Message is null)
                    continue;

                try
                {
                    var envelope = JsonSerializer.Deserialize<IntegrationEventMessage>(result.Message.Value)
                        ?? throw new InvalidOperationException("Empty integration event message");

                    await using var scope = _scopeFactory.CreateAsyncScope();
                    var session = scope.ServiceProvider.GetRequiredService<IDocumentSession>();
                    var updater = new ApplicationReadModelUpdater(session);
                    await updater.ApplyAsync(envelope.EventType, envelope.Payload);

                    consumer.Commit(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to apply integration event at {Offset}", result.Offset);
                }
            }
        }
        finally
        {
            consumer.Close();
        }
    }
}
