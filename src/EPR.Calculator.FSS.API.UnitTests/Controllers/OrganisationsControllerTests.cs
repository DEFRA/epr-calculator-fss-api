using System.Net;
using AutoFixture;
using EPR.Calculator.FSS.API.Controllers;
using EPR.Calculator.FSS.API.Models;
using EPR.Calculator.FSS.API.Services;
using EPR.Calculator.FSS.API.Validators;
using FluentAssertions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EPR.Calculator.FSS.API.UnitTests.Controllers;

[TestClass]
public class OrganisationsControllerTests
{
    private readonly NullLogger<OrganisationsController> _nullLogger = new();
    private readonly Mock<ValidationFailure> _validationFailureMock;
    private readonly Mock<ValidationResult> _validationResultMock;
    private Mock<OrganisationSearchFilterValidator> validatorMock = null!;
    private Mock<IOrganisationService> _organisationServiceMock = null!;
    private OrganisationsController _organisationController = null!;

    public OrganisationsControllerTests()
    {
        this.Fixture = new Fixture();
        this._organisationServiceMock = new Mock<IOrganisationService>();
        this._organisationController = new OrganisationsController(
            _organisationServiceMock.Object,
            new OrganisationSearchFilterValidator(),
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
    [DataRow("2025-01-30", "2025-26")]
    [DataRow(null, "2025-26")]
    [DataRow("2025-01-30", null)]
    [DataRow(null, null)]
    public async Task GetOrganisationsDetails_WithValidParams(string? createdOrModifiedAfter, string? financialYear)
    {
        // Arrange
        this._organisationServiceMock
           .Setup(service => service.GetOrganisationsDetails(It.IsAny<CancellationToken>(), It.IsAny<string?>(), It.IsAny<int?>()))
           .ReturnsAsync(new List<OrganisationDetails>
                            {
                                new OrganisationDetails
                                {
                                    OrganisationId = "12345",
                                    OrganisationName = "Test Org"
                                },
                            });

        // Act
        var result = await _organisationController.GetOrganisationsDetails(
            createdOrModifiedAfter: createdOrModifiedAfter,
            financialYear: financialYear) as ObjectResult;

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
    public async Task GetOrganisationsDetails_Empty()
    {
        // Arrange
        this._organisationServiceMock
           .Setup(service => service.GetOrganisationsDetails(It.IsAny<CancellationToken>(), It.IsAny<string?>(), It.IsAny<int?>()))
           .ReturnsAsync(new List<OrganisationDetails>());

        // Act
        var result = await _organisationController.GetOrganisationsDetails(
            createdOrModifiedAfter: "2021-01-30",
            financialYear: "2021-22") as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task GetOrganisationsDetails_Error()
    {
        this._organisationServiceMock
            .Setup(x => x.GetOrganisationsDetails(It.IsAny<CancellationToken>(), It.IsAny<string?>(), It.IsAny<int?>()))
            .ThrowsAsync(new HttpRequestException("InternalServerError exception", null, HttpStatusCode.InternalServerError));

        // Act
        var result = await this._organisationController.GetOrganisationsDetails(
            createdOrModifiedAfter: null,
            financialYear: null) as ActionResult;

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var statusCodeResult = result as StatusCodeResult;
        statusCodeResult?.StatusCode.Should().Be(500);
    }

    [TestMethod]
    [DataRow("bad")]
    [DataRow("1-1-1")]
    [DataRow("01-01-2025")]
    [DataRow("40-13-2025")]
    public async Task GetOrganisationsDetails_InvalidCreatedOrModifiedAfter(string createdOrModifiedAfter)
    {
        var result = await this._organisationController.GetOrganisationsDetails(
            createdOrModifiedAfter: createdOrModifiedAfter,
            financialYear: null) as BadRequestObjectResult;

        // Assert
        result.Should().NotBeNull();
        Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
    }

    [TestMethod]
    [DataRow("bad")]
    [DataRow("24-25")]
    [DataRow("2025-2026")]
    [DataRow("2025-27")]
    [DataRow("2025")]
    public async Task GetOrganisationsDetails_InvalidFinancialYear(string financialYear)
    {
        var result = await this._organisationController.GetOrganisationsDetails(
            createdOrModifiedAfter: null,
            financialYear: "bad") as BadRequestObjectResult;

        // Assert
        result.Should().NotBeNull();
        Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
    }
}
