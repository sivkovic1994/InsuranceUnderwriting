using InsuranceUnderwriting.Application;
using InsuranceUnderwriting.Application.Handlers;
using InsuranceUnderwriting.Domain;
using InsuranceUnderwriting.Domain.Services;

namespace InsuranceUnderwriting.Tests;

public class HandlerTests
{
    private readonly FakeApplicationRepository _repo = new();

    [Fact]
    public async Task SubmitApplicationHandler_AppendsApplicationSubmittedEvent()
    {
        var handler = new SubmitApplicationHandler(_repo);

        var id = await handler.Handle(new SubmitApplicationCommand("John Doe", "Auto"), CancellationToken.None);

        var app = await _repo.GetById(id);
        Assert.Equal("Submitted", app.Status);
        Assert.IsType<ApplicationSubmitted>(Assert.Single(_repo.EventsFor(id)));
    }

    [Fact]
    public async Task AssessRiskHandler_AppendsRiskAssessedEvent()
    {
        var id = await SubmitApplication("Auto");
        var handler = new AssessRiskHandler(_repo, new RiskAssessmentService());

        await handler.Handle(new AssessRiskCommand(id), CancellationToken.None);

        var app = await _repo.GetById(id);
        Assert.Equal("RiskAssessed", app.Status);
        Assert.Equal("Medium", app.RiskLevel);
    }

    [Fact]
    public async Task CalculatePremiumHandler_AppendsPremiumCalculatedEvent()
    {
        var id = await SubmitApplication("Auto");
        await new AssessRiskHandler(_repo, new RiskAssessmentService())
            .Handle(new AssessRiskCommand(id), CancellationToken.None);

        var handler = new CalculatePremiumHandler(_repo, new PremiumCalculationService());
        await handler.Handle(new CalculatePremiumCommand(id), CancellationToken.None);

        var app = await _repo.GetById(id);
        Assert.Equal("PremiumCalculated", app.Status);
        Assert.Equal(450m, app.Premium);
    }

    [Fact]
    public async Task ApprovePolicyHandler_AppendsPolicyApprovedEvent()
    {
        var id = await SubmitApplication("Auto");
        var handler = new ApprovePolicyHandler(_repo);

        await handler.Handle(new ApprovePolicyCommand(id), CancellationToken.None);

        var app = await _repo.GetById(id);
        Assert.Equal("Approved", app.Status);
    }

    [Fact]
    public async Task FullFlow_SubmitAssessCalculateApprove_ProducesExpectedFinalState()
    {
        var id = await SubmitApplication("Home");
        await new AssessRiskHandler(_repo, new RiskAssessmentService())
            .Handle(new AssessRiskCommand(id), CancellationToken.None);
        await new CalculatePremiumHandler(_repo, new PremiumCalculationService())
            .Handle(new CalculatePremiumCommand(id), CancellationToken.None);
        await new ApprovePolicyHandler(_repo)
            .Handle(new ApprovePolicyCommand(id), CancellationToken.None);

        var app = await _repo.GetById(id);
        Assert.Equal("Approved", app.Status);
        Assert.Equal("Low", app.RiskLevel);
        Assert.Equal(200m, app.Premium);
        Assert.Equal(4, _repo.EventsFor(id).Count);
    }

    private async Task<Guid> SubmitApplication(string insuranceType)
        => await new SubmitApplicationHandler(_repo)
            .Handle(new SubmitApplicationCommand("John Doe", insuranceType), CancellationToken.None);
}
