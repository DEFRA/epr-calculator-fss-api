namespace EPR.Calculator.FSS.API.UnitTests.Controllers;

using AutoFixture;
using Azure;
using EPR.Calculator.FSS.API.Common.Models;
using EPR.Calculator.FSS.API.Common.Services;
using EPR.Calculator.FSS.API.Controllers;
using EPR.Calculator.FSS.API.Validators;
using FluentAssertions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Net;

[TestClass]
public class OrganisationsControllerTests
{
    private readonly NullLogger<OrganisationsController> _nullLogger = new();
    private readonly Mock<ValidationFailure> _validationFailureMock;
    private readonly Mock<ValidationResult> _validationResultMock;
    private Mock<OrganisationSearchFilterValidator> validatorMock = null!;
    private Mock<IOrganisationService> _organisationServiceMock = null!;
    private OrganisationsController _organisationController = null!;
    private OrganisationSearchFilterValidator _mockValidator;

    public OrganisationsControllerTests()
    {
        this.Fixture = new Fixture();
        this._organisationServiceMock = new Mock<IOrganisationService>();
        this._mockValidator = new OrganisationSearchFilterValidator();
        this._organisationController = new OrganisationsController(
            _organisationServiceMock.Object,
            _mockValidator,
            _nullLogger)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext(),
            },
        };
        validatorMock = new Mock<OrganisationSearchFilterValidator>();
        _validationFailureMock = new Mock<ValidationFailure>();
        _validationResultMock = new Mock<ValidationResult>();
    }

    private IFixture Fixture { get; init; }

    [TestMethod]
    public async Task GetOrganisationsDetails_ReturnsOk()
    {
        // Arrange
        var createdOrModifiedAfter = "2021-01-30";
        this._organisationServiceMock
           .Setup(service => service.GetOrganisationsDetails(It.IsAny<string>()))
           .ReturnsAsync(new List<OrganisationDetails>
                            {
                                new OrganisationDetails
                                {
                                    OrganisationId = "12345",
                                    OrganisationName = "Test Org"
                                },
                            });

        // Act
        var result = await _organisationController.GetOrganisationsDetails(createdOrModifiedAfter) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result?.Value.Should().BeOfType<OrganisationsDetailsResponse>();

        var response = result.Value as OrganisationsDetailsResponse;
        response.OrganisationsDetails.Should().NotBeNullOrEmpty();
        response.OrganisationsDetails.Count.Should().Be(1);
        response.OrganisationsDetails[0].OrganisationId.Should().Be("12345");
        response.OrganisationsDetails[0].OrganisationName.Should().Be("Test Org");
    }

    [TestMethod]
    public async Task GetOrganisationsDetails_ReturnsNoContent()
    {
        // Arrange
        var organisationDetailsList = new List<OrganisationDetails>();
        this._organisationServiceMock.Setup(x =>
            x.GetOrganisationsDetails(It.IsAny<string>()))
            .ThrowsAsync(new HttpRequestException("Exception", null, HttpStatusCode.NotFound));

        // Act
        var result = await this._organisationController.GetOrganisationsDetails(It.IsAny<string>()) as NotFoundResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task GetOrganisationsDetailsReturnsStatus400BadRequest()
    {
        // Arrange
        var organisationDetailsList = new List<OrganisationDetails>();
        organisationDetailsList = null;

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
            .ThrowsAsync(new HttpRequestException("InternalServerError exception", null, HttpStatusCode.InternalServerError));

        // Act
        var result = await this._organisationController.GetOrganisationsDetails(createdOrModifiedAfter) as ActionResult;

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var statusCodeResult = result as StatusCodeResult;
        statusCodeResult?.StatusCode.Should().Be(500);
    }

    [TestMethod]
    public async Task GetOrganisationsDetailsStatus400InvalidDate()
    {
        // Arrange
        var createdOrModifiedAfter = "25-01-30";
        this._organisationServiceMock.Setup(x =>
            x.GetOrganisationsDetails(It.IsAny<string>()))
            .ThrowsAsync(new HttpRequestException("Test exception", null, HttpStatusCode.BadRequest));

        // Act
        var result = await this._organisationController.GetOrganisationsDetails(createdOrModifiedAfter) as BadRequestObjectResult; // as BadRequestResult;

        // Assert
        result.Should().NotBeNull();
        Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
    }

    [TestMethod]
    public async Task GetOrganisationsDetailsStatus404NotFound()
    {
        // Arrange
        var createdOrModifiedAfter = "2025-01-30";
        this._organisationServiceMock.Setup(x =>
            x.GetOrganisationsDetails(It.IsAny<string>()))
            .ThrowsAsync(new HttpRequestException("NotFound exception", null, HttpStatusCode.NotFound));

        // Act
        var result = await this._organisationController.GetOrganisationsDetails(createdOrModifiedAfter) as NotFoundResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be(404);
    }
}