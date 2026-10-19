using AutoFixture;
using AutoFixture.AutoMoq;
using EPR.Calculator.FSS.API.Controllers;
using EPR.Calculator.FSS.API.Services;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EPR.Calculator.FSS.API.UnitTests.Controllers;

[TestClass]
public class BillingControllerTests
{
    private readonly Mock<IValidator<int>> mockRunIdValidator = new();
    private readonly Mock<IBillingService> mockBillingService = new();
    private IFixture fixture = null!;
    private BillingController billingControllerUnderTest = null!;

    [TestInitialize]
    public void Setup()
    {
        fixture = new Fixture().Customize(new AutoMoqCustomization());

        billingControllerUnderTest = new BillingController(
            mockBillingService.Object,
            mockRunIdValidator.Object);
    }

    [TestMethod]
    public async Task CallGetBillingsDetails_Success()
    {
        // Arrange
        var runId = fixture.Create<int>();

        mockRunIdValidator.Setup(v => v.Validate(runId)).Returns(new ValidationResult());

        var billingsDetails = new FileStreamResult(new MemoryStream(), "application/json");

        mockBillingService.Setup(service => service.GetBillingFile(runId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(billingsDetails);

        // Act
        IActionResult result = await billingControllerUnderTest.GetBillingsDetails(runId, CancellationToken.None);

        // Assert
        using (new AssertionScope())
        {
            var fileResult = result.Should()
                .BeOfType<FileStreamResult>()
                .Which;

            fileResult.Should().BeSameAs(billingsDetails);

            mockBillingService.Verify(
                service => service.GetBillingFile(runId, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }

    [TestMethod]
    public async Task CallGetBillingsDetails_Returns400WhenValidationFails()
    {
        // Arrange
        var runId = fixture.Create<int>();

        var validationFailures = new List<ValidationFailure>
        {
            new("RunId", "RunId is invalid")
        };

        mockRunIdValidator.Setup(v => v.Validate(runId))
            .Returns(new ValidationResult(validationFailures));

        // Act
        IActionResult result = await billingControllerUnderTest.GetBillingsDetails(runId, CancellationToken.None);

        // Assert
        using (new AssertionScope())
        {
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Which;
            var problemDetails = badRequestResult.Value.Should().BeOfType<ProblemDetails>().Which;
            problemDetails.Detail.Should().Be("RunId is invalid");
            mockRunIdValidator.Verify(v => v.Validate(runId), Times.Once());
            mockBillingService.Verify(
                service => service.GetBillingFile(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }

    /// <summary>
    /// Checks that the controller returns a 404 when the service throws a FileNotFoundException.
    /// </summary>
    /// <returns>A <see cref="Task"/>.</returns>
    [TestMethod]
    public async Task CallGetBillingsDetails_Returns404WhenBillingsNotFound()
    {
        // Arrange
        var runId = fixture.Create<int>();

        mockRunIdValidator.Setup(v => v.Validate(runId)).Returns(new ValidationResult());
        mockBillingService.Setup(service => service.GetBillingFile(runId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FileNotFoundException());

        // Act
        IActionResult result = await billingControllerUnderTest.GetBillingsDetails(runId, CancellationToken.None);

        // Assert
        Assert.IsInstanceOfType<NotFoundObjectResult>(result);
    }
}
