using InsuranceUnderwriting.Domain;

namespace InsuranceUnderwriting.Tests;

public class InsuranceApplicationTests
{
    [Fact]
    public void Submit_WithValidData_ReturnsSubmittedApplicationAndEvent()
    {
        var (app, @event) = InsuranceApplication.Submit("John Doe", "Auto");

        Assert.Equal("Submitted", app.Status);
        Assert.Equal("John Doe", app.ClientName);
        Assert.Equal("Auto", app.InsuranceType);
        Assert.Equal(app.Id, @event.ApplicationId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Submit_WithoutClientName_ThrowsArgumentException(string? clientName)
    {
        Assert.Throws<ArgumentException>(() => InsuranceApplication.Submit(clientName!, "Auto"));
    }
}
