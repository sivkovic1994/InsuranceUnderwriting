using InsuranceUnderwriting.Domain.Services;

namespace InsuranceUnderwriting.Tests;

public class PremiumCalculationServiceTests
{
    private readonly PremiumCalculationService _sut = new();

    [Fact]
    public void CalculatePremium_ForAutoWithMediumRisk_AppliesMultiplier()
    {
        var premium = _sut.CalculatePremium("Medium", "Auto");

        Assert.Equal(450m, premium);
    }

    [Fact]
    public void CalculatePremium_ForAutoWithLowRisk_UsesBasePremium()
    {
        var premium = _sut.CalculatePremium("Low", "Auto");

        Assert.Equal(300m, premium);
    }

    [Fact]
    public void CalculatePremium_ForNonAutoWithMediumRisk_AppliesMultiplierToLowerBase()
    {
        var premium = _sut.CalculatePremium("Medium", "Home");

        Assert.Equal(300m, premium);
    }

    [Fact]
    public void CalculatePremium_ForNonAutoWithLowRisk_UsesBasePremium()
    {
        var premium = _sut.CalculatePremium("Low", "Home");

        Assert.Equal(200m, premium);
    }
}
