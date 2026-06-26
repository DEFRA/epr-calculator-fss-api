using System.Net.Mime;
using EPR.Calculator.FSS.API.Configs;
using EPR.Calculator.FSS.API.Controllers;
using EPR.Calculator.FSS.API.Helpers;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;

namespace EPR.Calculator.FSS.API.UnitTests.Controllers;

[TestClass]
public class TestOnlyControllerTests
{
    private Mock<IBlobStorageService> _mockBlobStorageService = new();
    private Mock<IValidator<int>> _mockRunIdValidator = new();
    private TestOnlyController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _controller = new TestOnlyController(
            _mockBlobStorageService.Object,
            _mockRunIdValidator.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [TestMethod]
    public async Task UploadBillingDetails_WhenValidRequest_UploadsFileAndReturnsOk()
    {
        // Arrange
        const int runId = 123;

        _mockRunIdValidator
            .Setup(v => v.Validate(runId))
            .Returns(new ValidationResult());

        _controller.Request.ContentType = "application/json";
        _controller.Request.Body = new MemoryStream("""{"field1":"value1"}"""u8.ToArray());

        var featureManagementSettings = Options.Create(new FeatureManagementSettings
        {
            EnableBillingUploadEndpoint = true
        });

        var expectedFileName = BillingFileNameHelper.Create(runId);

        // Act
        var result = await _controller.UploadBillingDetails(runId, featureManagementSettings);

        // Assert
        result.Should().BeOfType<OkResult>();

        _mockBlobStorageService.Verify(
            x => x.UploadFile(
                expectedFileName,
                It.IsAny<Stream>(),
                MediaTypeNames.Application.Json),
            Times.Once);
    }

    [TestMethod]
    public async Task UploadBillingDetails_WhenFeatureDisabled_ReturnsNotFound()
    {
        // Arrange
        var featureManagementSettings = Options.Create(new FeatureManagementSettings
        {
            EnableBillingUploadEndpoint = false
        });

        // Act
        var result = await _controller.UploadBillingDetails(123, featureManagementSettings);

        // Assert
        result.Should().BeOfType<NotFoundResult>();

        _mockBlobStorageService.Verify(
            x => x.UploadFile(
                It.IsAny<string>(),
                It.IsAny<Stream>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [TestMethod]
    public async Task UploadBillingDetails_WhenRunIdValidationFails_ReturnsBadRequest()
    {
        // Arrange
        const int runId = 123;

        _mockRunIdValidator
            .Setup(v => v.Validate(runId))
            .Returns(new ValidationResult(
            [
                new ValidationFailure("calculatorRunId", "Invalid run id")
            ]));

        _controller.Request.ContentType = MediaTypeNames.Application.Json;

        var featureManagementSettings = Options.Create(new FeatureManagementSettings
        {
            EnableBillingUploadEndpoint = true
        });

        // Act
        var result = await _controller.UploadBillingDetails(runId, featureManagementSettings);

        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Which;
        var problemDetails = badRequest.Value.Should().BeOfType<ProblemDetails>().Which;

        problemDetails.Detail.Should().Be("Invalid run id");

        _mockBlobStorageService.Verify(
            x => x.UploadFile(
                It.IsAny<string>(),
                It.IsAny<Stream>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [TestMethod]
    public async Task UploadBillingDetails_WhenContentTypeIsNotJson_ReturnsBadRequest()
    {
        // Arrange
        const int runId = 123;

        _mockRunIdValidator
            .Setup(v => v.Validate(runId))
            .Returns(new ValidationResult());

        _controller.Request.ContentType = MediaTypeNames.Text.Plain;

        var featureManagementSettings = Options.Create(new FeatureManagementSettings
        {
            EnableBillingUploadEndpoint = true
        });

        // Act
        var result = await _controller.UploadBillingDetails(runId, featureManagementSettings);

        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Which;
        var problemDetails = badRequest.Value.Should().BeOfType<ProblemDetails>().Which;

        problemDetails.Detail.Should().Be("Content-Type must be application/json");

        _mockBlobStorageService.Verify(
            x => x.UploadFile(
                It.IsAny<string>(),
                It.IsAny<Stream>(),
                It.IsAny<string>()),
            Times.Never);
    }
}
