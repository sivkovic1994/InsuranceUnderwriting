namespace InsuranceUnderwriting.Domain;

public class InsuranceApplication
{
    public Guid Id { get; private set; }
    public string ClientName { get; private set; } = string.Empty;
    public string InsuranceType { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public string? RiskLevel { get; private set; }
    public decimal? Premium { get; private set; }

    public static (InsuranceApplication, ApplicationSubmitted) Submit(string clientName, string insuranceType)
    {
        if (string.IsNullOrEmpty(clientName))
            throw new ArgumentException("Ime klijenta je obavezno");

        var id = Guid.NewGuid();
        var @event = new ApplicationSubmitted(id, clientName, insuranceType);
        var app = new InsuranceApplication();
        app.Apply(@event);
        return (app, @event);
    }

    public void Apply(ApplicationSubmitted e)
    {
        Id = e.ApplicationId;
        ClientName = e.ClientName;
        InsuranceType = e.InsuranceType;
        Status = "Submitted";
    }

    public void Apply(RiskAssessed e)
    {
        RiskLevel = e.RiskLevel;
        Status = "RiskAssessed";
    }

    public void Apply(PremiumCalculated e)
    {
        Premium = e.Premium;
        Status = "PremiumCalculated";
    }

    public void Apply(PolicyApproved e)
    {
        Status = "Approved";
    }
}
