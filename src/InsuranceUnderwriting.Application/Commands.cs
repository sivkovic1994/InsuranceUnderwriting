namespace InsuranceUnderwriting.Application;

public record SubmitApplicationCommand(string ClientName, string InsuranceType);
public record AssessRiskCommand(Guid ApplicationId);
public record CalculatePremiumCommand(Guid ApplicationId);
public record ApprovePolicyCommand(Guid ApplicationId);
