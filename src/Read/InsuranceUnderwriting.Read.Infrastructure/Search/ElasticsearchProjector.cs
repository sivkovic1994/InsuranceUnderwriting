using Elastic.Clients.Elasticsearch;
using InsuranceUnderwriting.Contracts;
using InsuranceUnderwriting.Read.Domain;

namespace InsuranceUnderwriting.Read.Infrastructure.Search;

// The Elasticsearch counterpart to ApplicationReadModelUpdater: applies the
// same integration events, but into a search-optimized index instead of the
// Marten lookup-by-id documents.
public class ElasticsearchProjector : IReadModelProjector
{
    public const string IndexName = "applications";

    private readonly ElasticsearchClient _client;
    public ElasticsearchProjector(ElasticsearchClient client) => _client = client;

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
        var doc = new ApplicationSearchDocument
        {
            Id = e.ApplicationId,
            ClientName = e.ClientName,
            InsuranceType = e.InsuranceType,
            Status = "Submitted",
            History = [$"Prijava podneta ({e.InsuranceType})"]
        };
        await Index(doc);
    }

    private async Task Apply(RiskAssessedIntegrationEvent e)
    {
        var doc = await Load(e.ApplicationId);
        if (doc is null)
            return;

        doc.RiskLevel = e.RiskLevel;
        doc.Status = "RiskAssessed";
        doc.History.Add($"Rizik procenjen: {e.RiskLevel} (score {e.RiskScore})");
        await Index(doc);
    }

    private async Task Apply(PremiumCalculatedIntegrationEvent e)
    {
        var doc = await Load(e.ApplicationId);
        if (doc is null)
            return;

        doc.Premium = e.Premium;
        doc.Status = "PremiumCalculated";
        doc.History.Add($"Premija izračunata: {e.Premium}");
        await Index(doc);
    }

    private async Task Apply(PolicyApprovedIntegrationEvent e)
    {
        var doc = await Load(e.ApplicationId);
        if (doc is null)
            return;

        doc.Status = "Approved";
        doc.History.Add($"Polisa odobrena {e.ApprovedAt:u}");
        await Index(doc);
    }

    private async Task<ApplicationSearchDocument?> Load(Guid id)
    {
        var response = await _client.GetAsync<ApplicationSearchDocument>(id.ToString(), g => g.Index(IndexName));
        return response.Found ? response.Source : null;
    }

    private async Task Index(ApplicationSearchDocument doc)
        => await _client.IndexAsync(doc, i => i.Index(IndexName).Id(doc.Id.ToString()));

    private static T Deserialize<T>(string payload)
        => System.Text.Json.JsonSerializer.Deserialize<T>(payload)
           ?? throw new InvalidOperationException($"Could not deserialize payload as {typeof(T).Name}");
}
