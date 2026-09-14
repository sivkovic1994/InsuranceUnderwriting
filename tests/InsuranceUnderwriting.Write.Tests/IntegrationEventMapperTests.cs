using InsuranceUnderwriting.Contracts;
using InsuranceUnderwriting.Write.Domain;
using InsuranceUnderwriting.Write.Infrastructure.Kafka;

namespace InsuranceUnderwriting.Write.Tests;

public class IntegrationEventMapperTests
{
    [Fact]
    public void ToIntegrationEvent_MapsApplicationSubmitted()
    {
        var domainEvent = new ApplicationSubmitted(Guid.NewGuid(), "John Doe", "Auto");

        var (eventType, payload) = IntegrationEventMapper.ToIntegrationEvent(domainEvent);

        Assert.Equal(IntegrationEventTypes.ApplicationSubmitted, eventType);
        var integrationEvent = Assert.IsType<ApplicationSubmittedIntegrationEvent>(payload);
        Assert.Equal(domainEvent.ApplicationId, integrationEvent.ApplicationId);
        Assert.Equal(domainEvent.ClientName, integrationEvent.ClientName);
        Assert.Equal(domainEvent.InsuranceType, integrationEvent.InsuranceType);
    }

    [Fact]
    public void ToIntegrationEvent_MapsRiskAssessed()
    {
        var domainEvent = new RiskAssessed(Guid.NewGuid(), "Medium", 45.5m);

        var (eventType, payload) = IntegrationEventMapper.ToIntegrationEvent(domainEvent);

        Assert.Equal(IntegrationEventTypes.RiskAssessed, eventType);
        var integrationEvent = Assert.IsType<RiskAssessedIntegrationEvent>(payload);
        Assert.Equal(domainEvent.RiskLevel, integrationEvent.RiskLevel);
        Assert.Equal(domainEvent.RiskScore, integrationEvent.RiskScore);
    }

    [Fact]
    public void ToIntegrationEvent_MapsPremiumCalculated()
    {
        var domainEvent = new PremiumCalculated(Guid.NewGuid(), 450m);

        var (eventType, payload) = IntegrationEventMapper.ToIntegrationEvent(domainEvent);

        Assert.Equal(IntegrationEventTypes.PremiumCalculated, eventType);
        var integrationEvent = Assert.IsType<PremiumCalculatedIntegrationEvent>(payload);
        Assert.Equal(domainEvent.Premium, integrationEvent.Premium);
    }

    [Fact]
    public void ToIntegrationEvent_MapsPolicyApproved()
    {
        var domainEvent = new PolicyApproved(Guid.NewGuid(), DateTime.UtcNow);

        var (eventType, payload) = IntegrationEventMapper.ToIntegrationEvent(domainEvent);

        Assert.Equal(IntegrationEventTypes.PolicyApproved, eventType);
        var integrationEvent = Assert.IsType<PolicyApprovedIntegrationEvent>(payload);
        Assert.Equal(domainEvent.ApprovedAt, integrationEvent.ApprovedAt);
    }
}
