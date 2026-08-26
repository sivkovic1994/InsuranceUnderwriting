using InsuranceUnderwriting.Application;
using InsuranceUnderwriting.Domain;

namespace InsuranceUnderwriting.Tests;

public class FakeApplicationRepository : IApplicationRepository
{
    private readonly Dictionary<Guid, List<object>> _streams = new();

    public Task<InsuranceApplication> GetById(Guid id)
    {
        var app = new InsuranceApplication();
        foreach (var @event in _streams[id])
            Apply(app, @event);

        return Task.FromResult(app);
    }

    public Task SaveNew(Guid id, object @event)
    {
        _streams[id] = [@event];
        return Task.CompletedTask;
    }

    public Task AppendEvent(Guid id, object @event)
    {
        _streams[id].Add(@event);
        return Task.CompletedTask;
    }

    public IReadOnlyList<object> EventsFor(Guid id) => _streams[id];

    private static void Apply(InsuranceApplication app, object @event)
    {
        switch (@event)
        {
            case ApplicationSubmitted e: app.Apply(e); break;
            case RiskAssessed e: app.Apply(e); break;
            case PremiumCalculated e: app.Apply(e); break;
            case PolicyApproved e: app.Apply(e); break;
        }
    }
}
