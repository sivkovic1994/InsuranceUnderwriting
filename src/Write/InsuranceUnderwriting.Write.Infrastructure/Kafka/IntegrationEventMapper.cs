using InsuranceUnderwriting.Contracts;
using InsuranceUnderwriting.Write.Domain;

namespace InsuranceUnderwriting.Write.Infrastructure.Kafka;

// Translates internal domain events into the public contract published on Kafka.
public static class IntegrationEventMapper
{
    public static (string EventType, object Payload) ToIntegrationEvent(object domainEvent) => domainEvent switch
    {
        ApplicationSubmitted e => (IntegrationEventTypes.ApplicationSubmitted,
            new ApplicationSubmittedIntegrationEvent(e.ApplicationId, e.ClientName, e.InsuranceType)),
        RiskAssessed e => (IntegrationEventTypes.RiskAssessed,
            new RiskAssessedIntegrationEvent(e.ApplicationId, e.RiskLevel, e.RiskScore)),
        PremiumCalculated e => (IntegrationEventTypes.PremiumCalculated,
            new PremiumCalculatedIntegrationEvent(e.ApplicationId, e.Premium)),
        PolicyApproved e => (IntegrationEventTypes.PolicyApproved,
            new PolicyApprovedIntegrationEvent(e.ApplicationId, e.ApprovedAt)),
        _ => throw new NotSupportedException($"No integration event mapping for {domainEvent.GetType().Name}")
    };
}
