using InsuranceUnderwriting.Domain;
using InsuranceUnderwriting.Domain.Services;
using MediatR;

namespace InsuranceUnderwriting.Application.Handlers;

public class CalculatePremiumHandler : IRequestHandler<CalculatePremiumCommand>
{
    private readonly IApplicationRepository _repo;
    private readonly PremiumCalculationService _premiumService;

    public CalculatePremiumHandler(IApplicationRepository repo, PremiumCalculationService premiumService)
    {
        _repo = repo;
        _premiumService = premiumService;
    }

    public async Task Handle(CalculatePremiumCommand cmd, CancellationToken cancellationToken)
    {
        var app = await _repo.GetById(cmd.ApplicationId);
        var premium = _premiumService.CalculatePremium(app.RiskLevel!, app.InsuranceType);
        await _repo.AppendEvent(cmd.ApplicationId, new PremiumCalculated(cmd.ApplicationId, premium));
    }
}
