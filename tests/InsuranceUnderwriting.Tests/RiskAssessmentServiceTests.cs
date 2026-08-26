using InsuranceUnderwriting.Domain.Services;

namespace InsuranceUnderwriting.Tests;

public class RiskAssessmentServiceTests
{
    private readonly RiskAssessmentService _sut = new();

    [Fact]
    public void AssessRisk_ForAuto_ReturnsMediumRiskWithExpectedScore()
    {
        var (riskLevel, riskScore) = _sut.AssessRisk("Auto", "John Doe");

        Assert.Equal("Medium", riskLevel);
        Assert.Equal(45.5m, riskScore);
    }

    [Theory]
    [InlineData("Home")]
    [InlineData("Life")]
    public void AssessRisk_ForNonAuto_ReturnsLowRiskWithExpectedScore(string insuranceType)
    {
        var (riskLevel, riskScore) = _sut.AssessRisk(insuranceType, "John Doe");

        Assert.Equal("Low", riskLevel);
        Assert.Equal(30m, riskScore);
    }
}
