using InsuranceUnderwriting.Domain;
using InsuranceUnderwriting.Domain.Services;
using MediatR;

namespace InsuranceUnderwriting.Application.Handlers;

public class AssessRiskHandler : IRequestHandler<AssessRiskCommand>
{
    private readonly IApplicationRepository _repo;
    private readonly RiskAssessmentService _riskService;

    public AssessRiskHandler(IApplicationRepository repo, RiskAssessmentService riskService)
    {
        _repo = repo;
        _riskService = riskService;
    }

    public async Task Handle(AssessRiskCommand cmd, CancellationToken cancellationToken)
    {
        var app = await _repo.GetById(cmd.ApplicationId);
        var (level, score) = _riskService.AssessRisk(app.InsuranceType, app.ClientName);
        await _repo.AppendEvent(cmd.ApplicationId, new RiskAssessed(cmd.ApplicationId, level, score));
    }
}
