namespace InsuranceUnderwriting.Contracts;

// Envelope actually sent as the Kafka message value: EventType selects how the
// Read service deserializes Payload (see IntegrationEventTypes).
public record IntegrationEventMessage(string EventType, string Payload);

public static class IntegrationEventTypes
{
    public const string ApplicationSubmitted = nameof(ApplicationSubmittedIntegrationEvent);
    public const string RiskAssessed = nameof(RiskAssessedIntegrationEvent);
    public const string PremiumCalculated = nameof(PremiumCalculatedIntegrationEvent);
    public const string PolicyApproved = nameof(PolicyApprovedIntegrationEvent);
}
