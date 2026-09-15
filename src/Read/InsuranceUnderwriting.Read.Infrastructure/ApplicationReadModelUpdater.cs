using InsuranceUnderwriting.Contracts;
using InsuranceUnderwriting.Read.Domain;
using Marten;

namespace InsuranceUnderwriting.Read.Infrastructure;

// The "projection" for the Read service: applies integration events consumed
// from Kafka onto the denormalized documents, upserting them via Marten used
// purely as a document store (no event sourcing on this side).
public class ApplicationReadModelUpdater : IReadModelProjector
{
    private readonly IDocumentSession _session;
    public ApplicationReadModelUpdater(IDocumentSession session) => _session = session;

    public async Task ApplyAsync(string eventType, string payload)
    {
        switch (eventType)
        {
            case IntegrationEventTypes.ApplicationSubmitted:
                await Apply(Deserialize<ApplicationSubmittedIntegrationEvent>(payload));
                break;
            case IntegrationEventTypes.RiskAssessed:
                await Apply(Deserialize<RiskAssessedIntegrationEvent>(payload));
                break;
            case IntegrationEventTypes.PremiumCalculated:
                await Apply(Deserialize<PremiumCalculatedIntegrationEvent>(payload));
                break;
            case IntegrationEventTypes.PolicyApproved:
                await Apply(Deserialize<PolicyApprovedIntegrationEvent>(payload));
                break;
            default:
                throw new NotSupportedException($"Unknown integration event type '{eventType}'");
        }
    }

    private async Task Apply(ApplicationSubmittedIntegrationEvent e)
    {
        _session.Store(new ApplicationReadModel
        {
            Id = e.ApplicationId,
            ClientName = e.ClientName,
            InsuranceType = e.InsuranceType,
            Status = "Submitted"
        });
        _session.Store(new ApplicationHistoryView
        {
            Id = e.ApplicationId,
            ClientName = e.ClientName,
            History = [$"Application submitted ({e.InsuranceType})"]
        });
        await _session.SaveChangesAsync();
    }

    private async Task Apply(RiskAssessedIntegrationEvent e)
    {
        var model = await _session.LoadAsync<ApplicationReadModel>(e.ApplicationId);
        if (model is not null)
        {
            model.RiskLevel = e.RiskLevel;
            model.Status = "RiskAssessed";
            _session.Store(model);
        }

        await AddHistoryEntry(e.ApplicationId, $"Risk assessed: {e.RiskLevel} (score {e.RiskScore})");
    }

    private async Task Apply(PremiumCalculatedIntegrationEvent e)
    {
        var model = await _session.LoadAsync<ApplicationReadModel>(e.ApplicationId);
        if (model is not null)
        {
            model.Premium = e.Premium;
            model.Status = "PremiumCalculated";
            _session.Store(model);
        }

        await AddHistoryEntry(e.ApplicationId, $"Premium calculated: {e.Premium}");
    }

    private async Task Apply(PolicyApprovedIntegrationEvent e)
    {
        var model = await _session.LoadAsync<ApplicationReadModel>(e.ApplicationId);
        if (model is not null)
        {
            model.Status = "Approved";
            _session.Store(model);
        }

        await AddHistoryEntry(e.ApplicationId, $"Policy approved {e.ApprovedAt:u}");
    }

    private async Task AddHistoryEntry(Guid applicationId, string entry)
    {
        var history = await _session.LoadAsync<ApplicationHistoryView>(applicationId);
        if (history is not null)
        {
            history.History.Add(entry);
            _session.Store(history);
        }

        await _session.SaveChangesAsync();
    }

    private static T Deserialize<T>(string payload)
        => System.Text.Json.JsonSerializer.Deserialize<T>(payload)
           ?? throw new InvalidOperationException($"Could not deserialize payload as {typeof(T).Name}");
}
