namespace InsuranceUnderwriting.Infrastructure.Projections;

public class ApplicationHistoryView
{
    public Guid Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public List<string> History { get; set; } = new();
}
