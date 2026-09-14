namespace InsuranceUnderwriting.Contracts;

// Public contract between the Write and Read microservices, carried over Kafka.
// Intentionally separate from the Write service's internal domain events so the
// two can evolve independently.
public record ApplicationSubmittedIntegrationEvent(Guid ApplicationId, string ClientName, string InsuranceType);
public record RiskAssessedIntegrationEvent(Guid ApplicationId, string RiskLevel, decimal RiskScore);
public record PremiumCalculatedIntegrationEvent(Guid ApplicationId, decimal Premium);
public record PolicyApprovedIntegrationEvent(Guid ApplicationId, DateTime ApprovedAt);
