using InsuranceUnderwriting.Application;
using InsuranceUnderwriting.Domain;
using Marten;

namespace InsuranceUnderwriting.Infrastructure;

public class MartenApplicationRepository : IApplicationRepository
{
    private readonly IDocumentSession _session;
    public MartenApplicationRepository(IDocumentSession session) => _session = session;

    public async Task<InsuranceApplication> GetById(Guid id)
        => await _session.Events.AggregateStreamAsync<InsuranceApplication>(id)
           ?? throw new InvalidOperationException($"Application {id} not found");

    public async Task SaveNew(Guid id, object @event)
    {
        _session.Events.StartStream<InsuranceApplication>(id, @event);
        await _session.SaveChangesAsync();
    }

    public async Task AppendEvent(Guid id, object @event)
    {
        _session.Events.Append(id, @event);
        await _session.SaveChangesAsync();
    }
}
