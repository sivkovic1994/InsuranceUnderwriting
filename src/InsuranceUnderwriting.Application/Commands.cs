using MediatR;

namespace InsuranceUnderwriting.Application;

public record SubmitApplicationCommand(string ClientName, string InsuranceType) : IRequest<Guid>;
public record AssessRiskCommand(Guid ApplicationId) : IRequest;
public record CalculatePremiumCommand(Guid ApplicationId) : IRequest;
public record ApprovePolicyCommand(Guid ApplicationId) : IRequest;
