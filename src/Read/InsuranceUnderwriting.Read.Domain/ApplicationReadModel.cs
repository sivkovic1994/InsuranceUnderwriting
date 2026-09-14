namespace InsuranceUnderwriting.Read.Domain;

// Denormalized current-state view of an application, rebuilt from integration
// events consumed off Kafka. Replaces querying the write-side aggregate directly.
public class ApplicationReadModel
{
    public Guid Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string InsuranceType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? RiskLevel { get; set; }
    public decimal? Premium { get; set; }
}
