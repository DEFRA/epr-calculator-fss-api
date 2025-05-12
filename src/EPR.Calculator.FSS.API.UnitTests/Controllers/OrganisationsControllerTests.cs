namespace EPR.Calculator.FSS.API.UnitTests.Controllers
{
    using System.Net;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.FSS.API.Common.Models;
    using EPR.Calculator.FSS.API.Common.Services;
    using EPR.Calculator.FSS.API.Controllers;
    using FluentAssertions;
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

        [TestInitialize]
        public void Setup()
        {
            _organisationServiceMock = new Mock<IOrganisationService>();
            _organisationController = new OrganisationsController(_organisationServiceMock.Object, _nullLogger)
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
            // Arrange
            _organisationServiceMock
                .Setup(service => service.GetOrganisationsDetails(It.IsAny<string>()))
                .ReturnsAsync(new List<OrganisationDetails> { new OrganisationDetails { OrganisationId ="test", OrganisationName = "Test Org" } });

            // Act
            var result = await _organisationController.GetOrganisationsDetails(It.IsAny<string>()) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
            result?.Value.Should().BeOfType<List<OrganisationDetails>>();
        }

        [TestMethod]
        public async Task GetOrganisationsDetails_ReturnsNoContent()
        {
            var myList = new List<OrganisationDetails>();

            // Arrange
            _organisationServiceMock
                .Setup(service => service.GetOrganisationsDetails(It.IsAny<string>()))
                .ReturnsAsync(myList);

            // Act
            var result = await _organisationController.GetOrganisationsDetails(It.IsAny<string>()) as ObjectResult;

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

       /* [TestMethod]
        public async Task GetOrganisationsDetails_ReturnsStatus400BadRequest()
        {
            // Arrange
            var myList = new List<OrganisationDetails>();
            myList = null;

            var response = Result<IReadOnlyCollection<OrganisationDetails>>.FailedResult("BadRequest", HttpStatusCode.BadRequest);

            _organisationServiceMock
                .Setup(service => service.GetOrganisationsDetails(It.IsAny<string>()))
                .ReturnsAsync(response);

            // Act
            var result = await _organisationController.GetOrganisationsDetails(It.IsAny<string>()) as ActionResult;

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            (result as BadRequestObjectResult).StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }*/

        [TestMethod]
        public async Task GetOrganisationsDetails_Status500InternalServerError()
        {
            // Arrange
            _organisationServiceMock.Setup(x =>
                x.GetOrganisationsDetails(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("Test exception", null, HttpStatusCode.InternalServerError));

            // Act
            var result = await _organisationController.GetOrganisationsDetails(It.IsAny<string>()) as BadRequestObjectResult;

            // Assert
            result?.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            result.Should().BeOfType<StatusCodeResult>();
            /*var statusCodeResult = result as BadRequestResult;
            statusCodeResult?.StatusCode.Should().Be(500);*/
            Assert.AreEqual(200, result.StatusCode);
        }
    }
}
