namespace InsuranceUnderwriting.Domain;

public record ApplicationSubmitted(Guid ApplicationId, string ClientName, string InsuranceType);
public record RiskAssessed(Guid ApplicationId, string RiskLevel, decimal RiskScore);
public record PremiumCalculated(Guid ApplicationId, decimal Premium);
public record PolicyApproved(Guid ApplicationId, DateTime ApprovedAt);
