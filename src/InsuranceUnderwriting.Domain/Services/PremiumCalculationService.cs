namespace InsuranceUnderwriting.Domain.Services;

public class PremiumCalculationService
{
    public decimal CalculatePremium(string riskLevel, string insuranceType)
    {
        decimal basePremium = insuranceType == "Auto" ? 300m : 200m;
        decimal multiplier = riskLevel == "Medium" ? 1.5m : 1.0m;
        return basePremium * multiplier;
    }
}
