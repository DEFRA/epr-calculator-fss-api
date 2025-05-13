namespace EPR.Calculator.FSS.API.UnitTests.Controllers
{
    using System.Net;
    using AutoFixture;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.FSS.API.Common.Models;
    using EPR.Calculator.FSS.API.Common.Services;
    using EPR.Calculator.FSS.API.Controllers;
    using FluentAssertions;
    using FluentValidation;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;

    [TestClass]
    public class OrganisationsControllerTests
    {
        private Mock<IOrganisationService> _organisationServiceMock = null!;
        private OrganisationsController _organisationController = null!;
        private readonly NullLogger<OrganisationsController> _nullLogger = new();

        private IFixture Fixture { get; init; }

        private bool validationResult { get; set; }

        private AbstractValidator<OrganisationSearchFilter> MockValidator { get; init; }

        public OrganisationsControllerTests()
        {
            this.Fixture = new Fixture();
            this._organisationServiceMock = new Mock<IOrganisationService>();
            this.MockValidator = new Common.Validators.OrganisationSearchFilterValidator();
            this._organisationController = new OrganisationsController(
                _organisationServiceMock.Object,
                this.MockValidator,
                _nullLogger)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext(),
                },
            };
        }

        [TestMethod]
        public async Task GetOrganisationsDetails_ReturnsOk()
        {
            var createdOrModifiedAfter = "2021-01-30";
            this._organisationServiceMock
                .Setup(service => service.GetOrganisationsDetails(It.IsAny<string>()))
                .ReturnsAsync(new List<OrganisationDetails>
                        { new OrganisationDetails
                            { OrganisationId = "test", OrganisationName = "Test Org" }, });

            // Act
            var result = await _organisationController.GetOrganisationsDetails(createdOrModifiedAfter) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
            result?.Value.Should().BeOfType<List<OrganisationDetails>>();
        }

        [TestMethod]
        public async Task GetOrganisationsDetailsReturnsNoContent()
        {
            var myList = new List<OrganisationDetails>();

            // Arrange
            this._organisationServiceMock
                .Setup(service => service.GetOrganisationsDetails(It.IsAny<string>()))
                .ReturnsAsync(myList);

            // Act
            var result = await this._organisationController.GetOrganisationsDetails(It.IsAny<string>()) as ObjectResult;

            // Assert
            result.Should().BeNull();
            result?.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
            result?.Value.Should().BeOfType<List<OrganisationDetails>>();
        }

        /*    [TestMethod]
            public async Task GetOrganisationsDetails_ReturnsStatus204NoContent()
            {
                var myList = new List<OrganisationDetails>();
                myList = null;

                // Arrange
                _organisationServiceMock
                    .Setup(service => service.GetOrganisationsDetails(It.IsAny<string>()))
                    .ReturnsAsync(myList);

                // Act
                var result = await _organisationController.GetOrganisationsDetails(It.IsAny<string>()) as NotFoundObjectResult;

                result.Should().NotBeNull();
                result?.Value.Should().Be("Organisation not found");
                result?.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            }*/

        [TestMethod]
        public async Task GetOrganisationsDetailsReturnsStatus400BadRequest()
        {
            // Arrange
            var myList = new List<OrganisationDetails>();
            myList = null;

            // Arrange
            this._organisationServiceMock.Setup(x =>
                x.GetOrganisationsDetails(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("Test exception", null, HttpStatusCode.BadRequest));

            // Act
            var result = await this._organisationController.GetOrganisationsDetails(null) as BadRequestResult;

            // Assert
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be(400);
        }

        [TestMethod]
        public async Task GetOrganisationsDetailsStatus500InternalServerError()
        {
            // Arrange
            var createdOrModifiedAfter = "2025-01-30";

            this._organisationServiceMock.Setup(x =>
                x.GetOrganisationsDetails(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("Test exception", null, HttpStatusCode.InternalServerError));

            // Act
            var result = await this._organisationController.GetOrganisationsDetails(createdOrModifiedAfter) as ActionResult;

            // Assert
            result.Should().BeOfType<StatusCodeResult>();
            var statusCodeResult = result as StatusCodeResult;
            statusCodeResult?.StatusCode.Should().Be(500);
        }

        [TestMethod]
        public async Task GetOrganisationsDetailsStatusInvalidDate()
        {
            // Arrange
            var createdOrModifiedAfter = "25-01-30";
            this._organisationServiceMock.Setup(x =>
                x.GetOrganisationsDetails(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("Test exception", null, HttpStatusCode.BadRequest));

            // Act
            var result = await this._organisationController.GetOrganisationsDetails(createdOrModifiedAfter) as BadRequestResult;

            // Assert
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be(400);
        }
    }
}
