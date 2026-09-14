using InsuranceUnderwriting.Write.Application;
using InsuranceUnderwriting.Write.Domain;
using Marten;

namespace InsuranceUnderwriting.Write.Infrastructure;

public class MartenApplicationRepository : IApplicationRepository
{
    private readonly IDocumentSession _session;
    private readonly IIntegrationEventPublisher _publisher;

    public MartenApplicationRepository(IDocumentSession session, IIntegrationEventPublisher publisher)
    {
        _session = session;
        _publisher = publisher;
    }

    public async Task<InsuranceApplication> GetById(Guid id)
        => await _session.Events.AggregateStreamAsync<InsuranceApplication>(id)
           ?? throw new InvalidOperationException($"Application {id} not found");

    public async Task SaveNew(Guid id, object @event)
    {
        _session.Events.StartStream<InsuranceApplication>(id, @event);
        await _session.SaveChangesAsync();
        await _publisher.PublishAsync(id, @event);
    }

    public async Task AppendEvent(Guid id, object @event)
    {
        _session.Events.Append(id, @event);
        await _session.SaveChangesAsync();
        await _publisher.PublishAsync(id, @event);
    }
}
