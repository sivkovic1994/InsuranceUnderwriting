namespace InsuranceUnderwriting.Domain.Services;

public class RiskAssessmentService
{
    public (string RiskLevel, decimal RiskScore) AssessRisk(string insuranceType, string clientName)
    {
        decimal score = insuranceType == "Auto" ? 45.5m : 30m;
        string level = score > 40 ? "Medium" : "Low";
        return (level, score);
    }
}
