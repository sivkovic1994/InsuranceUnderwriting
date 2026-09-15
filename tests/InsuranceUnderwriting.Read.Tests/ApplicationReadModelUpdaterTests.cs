using System.Text.Json;
using InsuranceUnderwriting.Contracts;
using InsuranceUnderwriting.Read.Domain;
using InsuranceUnderwriting.Read.Infrastructure;
using Marten;
using Moq;

namespace InsuranceUnderwriting.Read.Tests;

// Exercises the projection logic in ApplicationReadModelUpdater (the Marten
// side of the Kafka consumer pipeline) against a mocked IDocumentSession that
// behaves like an in-memory store, since Marten itself needs a real Postgres.
public class ApplicationReadModelUpdaterTests
{
    private readonly Mock<IDocumentSession> _session = new();
    private readonly Dictionary<Guid, ApplicationReadModel> _models = new();
    private readonly Dictionary<Guid, ApplicationHistoryView> _histories = new();
    private readonly ApplicationReadModelUpdater _sut;

    public ApplicationReadModelUpdaterTests()
    {
        _session.Setup(s => s.LoadAsync<ApplicationReadModel>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => _models.GetValueOrDefault(id));
        _session.Setup(s => s.LoadAsync<ApplicationHistoryView>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => _histories.GetValueOrDefault(id));
        _session.Setup(s => s.Store(It.IsAny<ApplicationReadModel[]>()))
            .Callback<ApplicationReadModel[]>(models =>
            {
                foreach (var m in models) _models[m.Id] = m;
            });
        _session.Setup(s => s.Store(It.IsAny<ApplicationHistoryView[]>()))
            .Callback<ApplicationHistoryView[]>(views =>
            {
                foreach (var v in views) _histories[v.Id] = v;
            });

        _sut = new ApplicationReadModelUpdater(_session.Object);
    }

    [Fact]
    public async Task ApplicationSubmitted_CreatesReadModelAndHistory()
    {
        var applicationId = Guid.NewGuid();
        await Apply(new ApplicationSubmittedIntegrationEvent(applicationId, "John Doe", "Auto"));

        var model = _models[applicationId];
        Assert.Equal("John Doe", model.ClientName);
        Assert.Equal("Auto", model.InsuranceType);
        Assert.Equal("Submitted", model.Status);

        var history = _histories[applicationId];
        Assert.Equal(["Application submitted (Auto)"], history.History);
    }

    [Fact]
    public async Task RiskAssessed_UpdatesExistingReadModelAndAppendsHistory()
    {
        var applicationId = Guid.NewGuid();
        await Apply(new ApplicationSubmittedIntegrationEvent(applicationId, "John Doe", "Auto"));

        await Apply(new RiskAssessedIntegrationEvent(applicationId, "Medium", 45.5m));

        var model = _models[applicationId];
        Assert.Equal("Medium", model.RiskLevel);
        Assert.Equal("RiskAssessed", model.Status);
        Assert.Equal(2, _histories[applicationId].History.Count);
        Assert.Equal("Risk assessed: Medium (score 45.5)", _histories[applicationId].History[1]);
    }

    [Fact]
    public async Task RiskAssessed_WithoutExistingReadModel_DoesNotThrowAndStillRecordsHistoryNoOp()
    {
        var applicationId = Guid.NewGuid();

        await Apply(new RiskAssessedIntegrationEvent(applicationId, "Medium", 45.5m));

        Assert.False(_models.ContainsKey(applicationId));
        Assert.False(_histories.ContainsKey(applicationId));
    }

    [Fact]
    public async Task PremiumCalculated_UpdatesPremiumAndStatus()
    {
        var applicationId = Guid.NewGuid();
        await Apply(new ApplicationSubmittedIntegrationEvent(applicationId, "John Doe", "Auto"));

        await Apply(new PremiumCalculatedIntegrationEvent(applicationId, 450m));

        var model = _models[applicationId];
        Assert.Equal(450m, model.Premium);
        Assert.Equal("PremiumCalculated", model.Status);
    }

    [Fact]
    public async Task PolicyApproved_SetsApprovedStatus()
    {
        var applicationId = Guid.NewGuid();
        await Apply(new ApplicationSubmittedIntegrationEvent(applicationId, "John Doe", "Auto"));

        var approvedAt = DateTime.UtcNow;
        await Apply(new PolicyApprovedIntegrationEvent(applicationId, approvedAt));

        Assert.Equal("Approved", _models[applicationId].Status);
        Assert.Equal($"Policy approved {approvedAt:u}", _histories[applicationId].History[1]);
    }

    [Fact]
    public async Task ApplyAsync_WithUnknownEventType_ThrowsNotSupportedException()
    {
        await Assert.ThrowsAsync<NotSupportedException>(
            () => _sut.ApplyAsync("SomeUnknownEvent", "{}"));
    }

    private Task Apply<T>(T integrationEvent) where T : notnull
    {
        var eventType = integrationEvent switch
        {
            ApplicationSubmittedIntegrationEvent => IntegrationEventTypes.ApplicationSubmitted,
            RiskAssessedIntegrationEvent => IntegrationEventTypes.RiskAssessed,
            PremiumCalculatedIntegrationEvent => IntegrationEventTypes.PremiumCalculated,
            PolicyApprovedIntegrationEvent => IntegrationEventTypes.PolicyApproved,
            _ => throw new InvalidOperationException("Unmapped test event type")
        };
        return _sut.ApplyAsync(eventType, JsonSerializer.Serialize(integrationEvent));
    }
}
