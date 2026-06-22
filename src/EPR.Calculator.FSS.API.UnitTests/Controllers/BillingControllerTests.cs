using AutoFixture;
using AutoFixture.AutoMoq;
using EPR.Calculator.FSS.API.Common;
using EPR.Calculator.FSS.API.Controllers;
using EPR.Calculator.FSS.API.Helpers;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Moq;

namespace EPR.Calculator.FSS.API.UnitTests.Controllers
{
    [TestClass]
    public class BillingControllerTests
    {
        private Mock<IValidator<int>> _mockRunIdValidator = new();
        private Mock<IBlobStorageService> _mockBlobStorageService = new();
        private IFixture _fixture = null!;
        private BillingController _billingControllerUnderTest;

        [TestInitialize]
        public void Setup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _billingControllerUnderTest = new BillingController(
                this._mockBlobStorageService.Object,
                new TelemetryClient(new TelemetryConfiguration
                {
                    TelemetryChannel = new Microsoft.ApplicationInsights.Channel.InMemoryChannel(),
                    DisableTelemetry = true,
                }),
                _mockRunIdValidator.Object);
        }

        [TestMethod]
        public async Task CallGetBillingsDetails_Success()
        {
            // Arrange
            var runId = _fixture.Create<int>();
            var expectedFileName = BillingFileNameHelper.Create(runId);

            _mockRunIdValidator.Setup(v => v.Validate(runId)).Returns(new ValidationResult());

            var billingsDetails = _fixture.Create<string>();

            _mockBlobStorageService.Setup(service => service.GetFileContents(expectedFileName))
                .ReturnsAsync(billingsDetails);

            // Act
            IActionResult result = await _billingControllerUnderTest.GetBillingsDetails(runId);

            // Assert
            using (new AssertionScope())
            {
                Assert.IsInstanceOfType(result, typeof(ContentResult));
                var contentResult = result as ContentResult;
                Assert.IsNotNull(contentResult);
                Assert.AreEqual(billingsDetails, contentResult.Content);
                _mockBlobStorageService.Verify(
                    service => service.GetFileContents(expectedFileName),
                    Times.Once);
            }
        }

        [TestMethod]
        public async Task CallGetBillingsDetails_Returns400WhenValidationFails()
        {
            // Arrange
            var runId = _fixture.Create<int>();
            var expectedFileName = BillingFileNameHelper.Create(runId);

            var billingsDetails = _fixture.Create<string>();

            var validationFailures = new List<ValidationFailure>
            {
                new("RunId", "RunId is invalid")
            };

            // Setup
            _mockRunIdValidator.Setup(v => v.Validate(runId))
                .Returns(new ValidationResult(validationFailures));

            _mockBlobStorageService.Setup(service => service.GetFileContents(expectedFileName))
                .ReturnsAsync(billingsDetails);

            // Act
            IActionResult result = await _billingControllerUnderTest.GetBillingsDetails(runId);

            // Assert
            using (new AssertionScope())
            {
                var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Which;
                var problemDetails = badRequestResult.Value.Should().BeOfType<ProblemDetails>().Which;
                problemDetails.Detail.Should().Be("RunId is invalid");
                _mockRunIdValidator.Verify(v => v.Validate(runId), Times.Once());
                _mockBlobStorageService.Verify(service => service.GetFileContents(expectedFileName), Times.Never);
            }
        }

        /// <summary>
        /// Checks that the controller returns a 404 when the service throws a KeyNotFoundException
        /// or a FileNotFoundException.
        /// </summary>
        /// <param name="exceptionType">The type of exception to test.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        [DataRow(typeof(FileNotFoundException))]
        public async Task CallGetBillingsDetails_Return400WhenBillingsNotFound(Type exceptionType)
        {
            // Arrange
            var runId = _fixture.Create<int>();
            var expectedFileName = BillingFileNameHelper.Create(runId);

            _mockRunIdValidator.Setup(v => v.Validate(runId)).Returns(new ValidationResult());
            _mockBlobStorageService.Setup(service => service.GetFileContents(expectedFileName))
                .Throws((Exception)Activator.CreateInstance(exceptionType)!);

            // Act
            IActionResult result = await _billingControllerUnderTest.GetBillingsDetails(runId);

            // Assert
            Assert.IsInstanceOfType<NotFoundObjectResult>(result);
        }

       /// <summary>
       /// Checks that the controller returns a 500 when the service throws an exception *other than
       /// the ones that indicate the billings were not found.
       /// </summary>
       /// <param name="exceptionType">The type of exception to test.</param>
       /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        [DataRow(typeof(Exception))]
        [DataRow(typeof(ApplicationException))]
        [DataRow(typeof(OutOfMemoryException))]
        public async Task CallGetBillingsDetails_Return500WhenServiceThrowsException(Type exceptionType)
        {
            // Arrange
            var runId = _fixture.Create<int>();
            var expectedFileName = BillingFileNameHelper.Create(runId);

            _mockRunIdValidator.Setup(v => v.Validate(runId)).Returns(new ValidationResult());
            _mockBlobStorageService.Setup(service => service.GetFileContents(expectedFileName))
                .Throws((Exception)Activator.CreateInstance(exceptionType)!);

            // Act
            IActionResult result = await _billingControllerUnderTest.GetBillingsDetails(runId);

            // Assert
            using (new AssertionScope())
            {
                Assert.IsInstanceOfType(result, typeof(IStatusCodeActionResult));
                var castedResult = result as IStatusCodeActionResult;
                Assert.IsNotNull(castedResult);
                Assert.AreEqual(
                    StatusCodes.Status500InternalServerError,
                    castedResult.StatusCode);
            }
        }
    }
}