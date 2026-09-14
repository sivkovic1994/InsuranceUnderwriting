using System.Text.Json;
using InsuranceUnderwriting.Contracts;

namespace InsuranceUnderwriting.Read.Tests;

// Verifies the wire contract the Read service's Kafka consumer relies on:
// an IntegrationEventMessage envelope whose Payload round-trips into the
// concrete integration event named by EventType.
public class IntegrationEventMessageSerializationTests
{
    [Fact]
    public void ApplicationSubmittedIntegrationEvent_RoundTripsThroughEnvelope()
    {
        var original = new ApplicationSubmittedIntegrationEvent(Guid.NewGuid(), "John Doe", "Auto");
        var envelope = new IntegrationEventMessage(
            IntegrationEventTypes.ApplicationSubmitted,
            JsonSerializer.Serialize(original));

        var envelopeJson = JsonSerializer.Serialize(envelope);
        var deserializedEnvelope = JsonSerializer.Deserialize<IntegrationEventMessage>(envelopeJson)!;
        var result = JsonSerializer.Deserialize<ApplicationSubmittedIntegrationEvent>(deserializedEnvelope.Payload)!;

        Assert.Equal(IntegrationEventTypes.ApplicationSubmitted, deserializedEnvelope.EventType);
        Assert.Equal(original, result);
    }

    [Fact]
    public void RiskAssessedIntegrationEvent_RoundTripsThroughEnvelope()
    {
        var original = new RiskAssessedIntegrationEvent(Guid.NewGuid(), "Medium", 45.5m);
        var envelope = new IntegrationEventMessage(
            IntegrationEventTypes.RiskAssessed,
            JsonSerializer.Serialize(original));

        var result = JsonSerializer.Deserialize<RiskAssessedIntegrationEvent>(
            JsonSerializer.Deserialize<IntegrationEventMessage>(JsonSerializer.Serialize(envelope))!.Payload)!;

        Assert.Equal(original, result);
    }

    [Fact]
    public void PremiumCalculatedIntegrationEvent_RoundTripsThroughEnvelope()
    {
        var original = new PremiumCalculatedIntegrationEvent(Guid.NewGuid(), 450m);
        var envelope = new IntegrationEventMessage(
            IntegrationEventTypes.PremiumCalculated,
            JsonSerializer.Serialize(original));

        var result = JsonSerializer.Deserialize<PremiumCalculatedIntegrationEvent>(
            JsonSerializer.Deserialize<IntegrationEventMessage>(JsonSerializer.Serialize(envelope))!.Payload)!;

        Assert.Equal(original, result);
    }

    [Fact]
    public void PolicyApprovedIntegrationEvent_RoundTripsThroughEnvelope()
    {
        var original = new PolicyApprovedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow);
        var envelope = new IntegrationEventMessage(
            IntegrationEventTypes.PolicyApproved,
            JsonSerializer.Serialize(original));

        var result = JsonSerializer.Deserialize<PolicyApprovedIntegrationEvent>(
            JsonSerializer.Deserialize<IntegrationEventMessage>(JsonSerializer.Serialize(envelope))!.Payload)!;

        Assert.Equal(original, result);
    }
}
