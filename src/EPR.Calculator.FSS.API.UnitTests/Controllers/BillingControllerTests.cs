using AutoFixture;
using EPR.Calculator.FSS.API.Common;
using EPR.Calculator.FSS.API.Common.UnitTests.Validators;
using EPR.Calculator.FSS.API.Common.Validators;
using EPR.Calculator.FSS.API.Controllers;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;

namespace EPR.Calculator.FSS.API.UnitTests.Controllers
{
    [TestClass]
    public class BillingControllerTests
    {
        public BillingControllerTests()
        {
            this.Fixture = new Fixture();
            this.MockBillingService = new Mock<IBillingService>();

            this.MockRunIdValidator = new MockRunIdValidator(() => this.ValidationResult);

            this.TestClass = new BillingController(
                this.MockBillingService.Object,
                new TelemetryClient(new TelemetryConfiguration
                {
                    TelemetryChannel = new Microsoft.ApplicationInsights.Channel.InMemoryChannel(),
                    DisableTelemetry = true,
                }),
                this.MockRunIdValidator);
        }

        private IFixture Fixture { get; init; }

        private Mock<IBillingService> MockBillingService { get; init; }

        private RunIdValidator MockRunIdValidator { get; init; }

        private BillingController TestClass { get; init; }

        private bool ValidationResult { get; set; }

        [TestMethod]
        public async Task CallGetBillingsDetails_Success()
        {
            // Arrange
            var runId = this.Fixture.Create<int>();
            var billingsDetails = this.Fixture.Create<string>();
            this.MockBillingService.Setup(service => service.GetBillingData(runId))
                .ReturnsAsync(billingsDetails);
            this.ValidationResult = true;

            // Act
            var result = await this.TestClass.GetBillingsDetails(runId);

            // Assert
            Assert.IsInstanceOfType<ContentHttpResult>(result);
            var castedResult = (ContentHttpResult)result;
            Assert.AreEqual(billingsDetails, castedResult.ResponseContent);
        }

        [TestMethod]
        public async Task CallGetBillingsDetails_Returns400WhenValidationFails()
        {
            // Arrange
            var runId = this.Fixture.Create<int>();
            var billingsDetails = this.Fixture.Create<string>();
            this.MockBillingService.Setup(service => service.GetBillingData(runId))
                .ReturnsAsync(billingsDetails);
            this.ValidationResult = false;
            this.TestClass.ModelState.AddModelError("RunId", "Invalid RunId");

            // Act
            var result = await this.TestClass.GetBillingsDetails(runId);

            // Assert
            Assert.IsInstanceOfType<BadRequest>(result);
        }

        /// <summary>
        /// Checks that the controller returns a 404 when the service throws a KeyNotFoundException
        /// or a FileNotFoundException.
        /// </summary>
        /// <param name="exceptionType">The type of exception to test.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        [DataRow(typeof(KeyNotFoundException))]
        [DataRow(typeof(FileNotFoundException))]
        public async Task CallGetBillingsDetails_Return400WhenBillingsNotFound(Type exceptionType)
        {
            // Arrange
            var runId = this.Fixture.Create<int>();
            this.MockBillingService.Setup(service => service.GetBillingData(runId))
                .Throws((Exception)Activator.CreateInstance(exceptionType)!);
            this.ValidationResult = true;

            // Act
            var result = await this.TestClass.GetBillingsDetails(runId);

            // Assert
            Assert.IsInstanceOfType<BadRequest>(result);
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
            var runId = this.Fixture.Create<int>();
            this.MockBillingService.Setup(service => service.GetBillingData(runId))
                .Throws((Exception)Activator.CreateInstance(exceptionType)!);
            this.ValidationResult = true;

            // Act
            var result = await this.TestClass.GetBillingsDetails(runId);

            // Assert
            Assert.IsInstanceOfType<StatusCodeHttpResult>(result);
            var castedResult = (StatusCodeHttpResult)result;
            Assert.AreEqual(
                (int)HttpStatusCode.InternalServerError,
                castedResult.StatusCode);
        }
    }
}