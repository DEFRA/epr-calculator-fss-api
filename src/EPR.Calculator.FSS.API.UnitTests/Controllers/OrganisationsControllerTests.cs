using System.Net;
using EPR.Calculator.FSS.API.Controllers;
using EPR.Calculator.FSS.API.Models;
using EPR.Calculator.FSS.API.Services;
using FluentAssertions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EPR.Calculator.FSS.API.UnitTests.Controllers;

[TestClass]
public class OrganisationsControllerTests
{
    private readonly Mock<IOrganisationService> organisationServiceMock;
    private readonly OrganisationsController organisationController = null!;

    public OrganisationsControllerTests()
    {
        this.organisationServiceMock = new Mock<IOrganisationService>();
        this.organisationController = new OrganisationsController(organisationServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext(),
            },
        };
    }

    public async Task GetOrganisationsDetails_WithValidParams()
    {
        // Arrange
        this.organisationServiceMock
           .Setup(service => service.GetOrganisationsDetails(It.IsAny<DateTime?>(), It.IsAny<RelativeYear?>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(new List<OrganisationDetails>
                            {
                                new OrganisationDetails
                                {
                                    OrganisationId = "12345",
                                    FinancialYear = "2024-25",
                                    ApprovedDate = DateTime.Now,
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
            approvedAfter: null,
            financialYear: null) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result?.Value.Should().BeOfType<OrganisationsDetailsResponse>();

        var response = result!.Value as OrganisationsDetailsResponse;
        response.Should().NotBeNull();

        var orgs = response.OrganisationsDetails.ToList();
        orgs.Should().NotBeNullOrEmpty();
        orgs.Count.Should().Be(1);
        orgs[0].OrganisationId.Should().Be("12345");
        orgs[0].OrganisationName.Should().Be("Test Org");
    }

    [TestMethod]
    public async Task GetOrganisationsDetails_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        this.organisationController.ModelState.AddModelError(
            "approvedAfter",
            "The value 'invalid-date' is not valid.");

        // Act
        var result = await this.organisationController.GetOrganisationsDetails(
            approvedAfter: null,
            financialYear: null) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        result.Value.Should().BeOfType<ApiError>();

        var error = result.Value as ApiError;
        error.Should().NotBeNull();
        error.Error.Should().Be("Bad Request");
        error.ErrorCode.Should().Be("invalid_request");
        error.StatusCode.Should().Be(400);
        error.Message.Should().Contain("invalid-date");
    }
}
