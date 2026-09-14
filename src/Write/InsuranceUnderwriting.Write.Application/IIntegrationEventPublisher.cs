namespace InsuranceUnderwriting.Write.Application;

// Published to Kafka so the Read microservice can update its denormalized model.
public interface IIntegrationEventPublisher
{
    Task PublishAsync(Guid applicationId, object domainEvent);
}
