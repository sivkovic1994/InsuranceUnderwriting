namespace InsuranceUnderwriting.Read.Domain;

// Denormalized document indexed into Elasticsearch, optimized for full-text
// search (client name, insurance type, history) rather than lookup by id -
// that's what ApplicationReadModel/ApplicationHistoryView (Marten) are for.
public class ApplicationSearchDocument
{
    public Guid Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string InsuranceType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? RiskLevel { get; set; }
    public decimal? Premium { get; set; }
    public List<string> History { get; set; } = new();
}
