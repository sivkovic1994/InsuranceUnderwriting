using InsuranceUnderwriting.Domain;
using Marten.Events.Aggregation;

namespace InsuranceUnderwriting.Infrastructure.Projections;

// A real projection: builds a read model shaped differently from the aggregate (step history),
// unlike Snapshot<InsuranceApplication>() which just stores the aggregate's current state.
public class ApplicationHistoryProjection : SingleStreamProjection<ApplicationHistoryView, Guid>
{
    public void Apply(ApplicationHistoryView view, ApplicationSubmitted e)
    {
        view.Id = e.ApplicationId;
        view.ClientName = e.ClientName;
        view.History.Add($"Prijava podneta ({e.InsuranceType})");
    }

    public void Apply(ApplicationHistoryView view, RiskAssessed e)
        => view.History.Add($"Rizik procenjen: {e.RiskLevel} (score {e.RiskScore})");

    public void Apply(ApplicationHistoryView view, PremiumCalculated e)
        => view.History.Add($"Premija izračunata: {e.Premium}");

    public void Apply(ApplicationHistoryView view, PolicyApproved e)
        => view.History.Add($"Polisa odobrena {e.ApprovedAt:u}");
}
