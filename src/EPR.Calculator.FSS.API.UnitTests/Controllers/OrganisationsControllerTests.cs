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
    private readonly NullLogger<OrganisationsController> nullLogger = new();
    private readonly Mock<IOrganisationService> organisationServiceMock;
    private readonly OrganisationsController organisationController = null!;

    public OrganisationsControllerTests()
    {
        this.Fixture = new Fixture();
        this.organisationServiceMock = new Mock<IOrganisationService>();
        this.organisationController = new OrganisationsController(
            organisationServiceMock.Object,
            new OrganisationSearchFilterValidator(),
            nullLogger)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext(),
            },
        };
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
        this.organisationServiceMock
           .Setup(service => service.GetOrganisationsDetails(It.IsAny<CancellationToken>(), It.IsAny<string?>(), It.IsAny<int?>()))
           .ReturnsAsync(new List<OrganisationDetails>
                            {
                                new OrganisationDetails
                                {
                                    OrganisationId = "12345",
                                    FinancialYear = "2024-25",
                                    OrganisationName = "Test Org",
                                    OrganisationTradingName = "Test Trading",
                                    CompaniesHouseNumber = "12345678",
                                    HomeNationCode = "EN",
                                    ServiceOfNoticeAddrLine1 = "1 Test Street",
                                    ServiceOfNoticeAddrLine2 = "Suite 100",
                                    ServiceOfNoticeAddrCity = "Test City",
                                    ServiceOfNoticeAddrCounty = "Test County",
                                    ServiceOfNoticeAddrCountry = "England",
                                    ServiceOfNoticeAddrPostcode = "TE1 1ST",
                                    ServiceOfNoticeAddrPhoneNumber = "01234567890",
                                    SoleTraderFirstName = "John",
                                    SoleTraderLastName = "Smith",
                                    SoleTraderPhoneNumber = "07123456789",
                                    SoleTraderEmail = "john.smith@test.com",
                                    PrimaryContactPersonFirstName = "Jane",
                                    PrimaryContactPersonLastName = "Doe",
                                    PrimaryContactPersonPhoneNumber = "07987654321",
                                    PrimaryContactPersonEmail = "jane.doe@test.com",
                                    SubsidiaryDetails = new List<SubsidiaryDetails>()
                                }
                            });

        // Act
        var result = await organisationController.GetOrganisationsDetails(
            createdOrModifiedAfter: createdOrModifiedAfter,
            financialYear: financialYear) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result?.Value.Should().BeOfType<OrganisationsDetailsResponse>();

        var response = result!.Value as OrganisationsDetailsResponse;
        result.Should().NotBeNull();
        response!.OrganisationsDetails.Should().NotBeNullOrEmpty();
        response!.OrganisationsDetails.Count.Should().Be(1);
        response!.OrganisationsDetails[0].OrganisationId.Should().Be("12345");
        response!.OrganisationsDetails[0].OrganisationName.Should().Be("Test Org");
    }

    [TestMethod]
    public async Task GetOrganisationsDetails_Empty()
    {
        // Arrange
        this.organisationServiceMock
           .Setup(service => service.GetOrganisationsDetails(It.IsAny<CancellationToken>(), It.IsAny<string?>(), It.IsAny<int?>()))
           .ReturnsAsync(new List<OrganisationDetails>());

        // Act
        var result = await organisationController.GetOrganisationsDetails(
            createdOrModifiedAfter: "2021-01-30",
            financialYear: "2021-22") as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task GetOrganisationsDetails_Error()
    {
        this.organisationServiceMock
            .Setup(x => x.GetOrganisationsDetails(It.IsAny<CancellationToken>(), It.IsAny<string?>(), It.IsAny<int?>()))
            .ThrowsAsync(new HttpRequestException("InternalServerError exception", null, HttpStatusCode.InternalServerError));

        // Act
        var result = await this.organisationController.GetOrganisationsDetails(
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
        var result = await this.organisationController.GetOrganisationsDetails(
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
        var result = await this.organisationController.GetOrganisationsDetails(
            createdOrModifiedAfter: null,
            financialYear: "bad") as BadRequestObjectResult;

        // Assert
        result.Should().NotBeNull();
        Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
    }
}
