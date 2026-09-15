namespace InsuranceUnderwriting.Read.Infrastructure;

// Applies one integration event onto a read model. Each implementation owns a
// different denormalized store (Marten documents, Elasticsearch index, ...);
// the Kafka consumer runs every registered projector for every event.
public interface IReadModelProjector
{
    Task ApplyAsync(string eventType, string payload);
}
