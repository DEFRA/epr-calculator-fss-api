namespace EPR.Calculator.FSS.API.Common.UnitTests.Services;

using EPR.Calculator.FSS.API.Common.Data;
using EPR.Calculator.FSS.API.Common.Data.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Moq;

[TestClass]
public class OrganisationServiceTests
{
    private readonly OrganisationService _organisationService;
    private readonly Mock<SynapseDbContext> _mockSynapseDbContext;
    private readonly Mock<ILogger<OrganisationService>> _mockLogger;

    public OrganisationServiceTests()
    {
        _mockSynapseDbContext = new Mock<SynapseDbContext>();
        _mockLogger = new Mock<ILogger<OrganisationService>>();

        _organisationService = new OrganisationService(_mockSynapseDbContext.Object, _mockLogger.Object);
    }

    [TestMethod]
    public async Task GetOrganisationsDetails_ReturnsEmptyResult_WhenNoData()
    {
        // Arrange
        var emptyData = new List<AcceptedGrantedOrgDataResponseModel>();
        _mockSynapseDbContext
            .Setup(ctx => ctx.RunSqlAsync<AcceptedGrantedOrgDataResponseModel>(It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
            .ReturnsAsync(emptyData);

        // Act
        var result = await _organisationService.GetOrganisationsDetails();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetOrganisationsDetails_ValidRequestWithTwoOrganisations_ReturnsOrganisationDetails()
    {
        // Arrange
        var expectedData = new List<AcceptedGrantedOrgDataResponseModel>
        {
            new ()
            {
                OrganisationId = 12345,
                SubsidiaryId = null,
                OrganisationName = "Example Organisation Ltd",
                TradingName = "Example Trading Name",
                CompaniesHouseNumber = "12345678",
                HomeNationCode = "ENG",
                ServiceOfNoticeAddrLine1 = "123 Example Street",
                ServiceOfNoticeAddrLine2 = "Suite 456",
                ServiceOfNoticeAddrCity = "London",
                ServiceOfNoticeAddrCounty = "Greater London",
                ServiceOfNoticeAddrCountry = "United Kingdom",
                ServiceOfNoticeAddrPostcode = "E1 6AN",
                ServiceOfNoticeAddrPhoneNumber = "+44 20 7946 0123",
                SoleTraderFirstName = "John",
                SoleTraderLastName = "Doe",
                SoleTraderPhoneNumber = "+44 7911 123456",
                SoleTraderEmail = "john.doe@example.com",
                PrimaryContactPersonFirstName = "Jane",
                PrimaryContactPersonLastName = "Smith",
                PrimaryContactPersonPhoneNumber = "+44 7911 654321",
                PrimaryContactPersonEmail = "jane.smith@example.com",
            },
            new ()
            {
                OrganisationId = 67890,
                SubsidiaryId = null,
                OrganisationName = "Example Organisation Ltd 2",
                TradingName = "Trading Name 2",
                CompaniesHouseNumber = "00012345",
                HomeNationCode = "NI",
                ServiceOfNoticeAddrLine1 = "123 West Street",
                ServiceOfNoticeAddrLine2 = "Room 0",
                ServiceOfNoticeAddrCity = "Belfast",
                ServiceOfNoticeAddrCounty = "Greater Belfast",
                ServiceOfNoticeAddrCountry = "United Kingdom",
                ServiceOfNoticeAddrPostcode = "BT1 1AA,",
                ServiceOfNoticeAddrPhoneNumber = "+44 028 7946 0123",
                SoleTraderFirstName = "Bob",
                SoleTraderLastName = "Darling",
                SoleTraderPhoneNumber = "+44 7911 999999",
                SoleTraderEmail = "bob.darling@example.com",
                PrimaryContactPersonFirstName = "Janine",
                PrimaryContactPersonLastName = "Smitherson",
                PrimaryContactPersonPhoneNumber = "+44 7911 123123",
                PrimaryContactPersonEmail = "j.smitherson@example.com",
            },
        };

        _mockSynapseDbContext
            .Setup(ctx => ctx.RunSqlAsync<AcceptedGrantedOrgDataResponseModel>(It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
            .ReturnsAsync(expectedData);

        // Act
        var result = await _organisationService.GetOrganisationsDetails();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);

        var firstOrganisation = result.First();

        Assert.AreEqual("12345", firstOrganisation.OrganisationId);
        Assert.AreEqual("Example Organisation Ltd", firstOrganisation.OrganisationName);
        Assert.AreEqual("Example Trading Name", firstOrganisation.OrganisationTradingName);
        Assert.AreEqual("12345678", firstOrganisation.CompaniesHouseNumber);
        Assert.AreEqual("ENG", firstOrganisation.HomeNationCode);
        Assert.AreEqual("123 Example Street", firstOrganisation.ServiceOfNoticeAddrLine1);
        Assert.AreEqual("Suite 456", firstOrganisation.ServiceOfNoticeAddrLine2);
        Assert.AreEqual("London", firstOrganisation.ServiceOfNoticeAddrCity);
        Assert.AreEqual("Greater London", firstOrganisation.ServiceOfNoticeAddrCounty);
        Assert.AreEqual("United Kingdom", firstOrganisation.ServiceOfNoticeAddrCountry);
        Assert.AreEqual("E1 6AN", firstOrganisation.ServiceOfNoticeAddrPostcode);
        Assert.AreEqual("+44 20 7946 0123", firstOrganisation.ServiceOfNoticeAddrPhoneNumber);
        Assert.AreEqual("John", firstOrganisation.SoleTraderFirstName);
        Assert.AreEqual("Doe", firstOrganisation.SoleTraderLastName);
        Assert.AreEqual("+44 7911 123456", firstOrganisation.SoleTraderPhoneNumber);
        Assert.AreEqual("john.doe@example.com", firstOrganisation.SoleTraderEmail);
        Assert.AreEqual("Jane", firstOrganisation.PrimaryContactPersonFirstName);
        Assert.AreEqual("Smith", firstOrganisation.PrimaryContactPersonLastName);
        Assert.AreEqual("+44 7911 654321", firstOrganisation.PrimaryContactPersonPhoneNumber);
        Assert.AreEqual("jane.smith@example.com", firstOrganisation.PrimaryContactPersonEmail);

        Assert.IsNotNull(firstOrganisation.SubsidiaryDetails);
        Assert.AreEqual(0, firstOrganisation.SubsidiaryDetails.Count);

        var secondOrganisation = result.Skip(1).First();

        Assert.AreEqual("67890", secondOrganisation.OrganisationId);
        Assert.AreEqual("Example Organisation Ltd 2", secondOrganisation.OrganisationName);
        Assert.AreEqual("Trading Name 2", secondOrganisation.OrganisationTradingName);
        Assert.AreEqual("00012345", secondOrganisation.CompaniesHouseNumber);
        Assert.AreEqual("NI", secondOrganisation.HomeNationCode);
        Assert.AreEqual("123 West Street", secondOrganisation.ServiceOfNoticeAddrLine1);
        Assert.AreEqual("Room 0", secondOrganisation.ServiceOfNoticeAddrLine2);
        Assert.AreEqual("Belfast", secondOrganisation.ServiceOfNoticeAddrCity);
        Assert.AreEqual("Greater Belfast", secondOrganisation.ServiceOfNoticeAddrCounty);
        Assert.AreEqual("United Kingdom", secondOrganisation.ServiceOfNoticeAddrCountry);
        Assert.AreEqual("BT1 1AA,", secondOrganisation.ServiceOfNoticeAddrPostcode);
        Assert.AreEqual("+44 028 7946 0123", secondOrganisation.ServiceOfNoticeAddrPhoneNumber);
        Assert.AreEqual("Bob", secondOrganisation.SoleTraderFirstName);
        Assert.AreEqual("Darling", secondOrganisation.SoleTraderLastName);
        Assert.AreEqual("+44 7911 999999", secondOrganisation.SoleTraderPhoneNumber);
        Assert.AreEqual("bob.darling@example.com", secondOrganisation.SoleTraderEmail);
        Assert.AreEqual("Janine", secondOrganisation.PrimaryContactPersonFirstName);
        Assert.AreEqual("Smitherson", secondOrganisation.PrimaryContactPersonLastName);
        Assert.AreEqual("+44 7911 123123", secondOrganisation.PrimaryContactPersonPhoneNumber);
        Assert.AreEqual("j.smitherson@example.com", secondOrganisation.PrimaryContactPersonEmail);

        Assert.IsNotNull(secondOrganisation.SubsidiaryDetails);
        Assert.AreEqual(0, secondOrganisation.SubsidiaryDetails.Count);

        _mockSynapseDbContext.Verify(
            ctx => ctx.RunSqlAsync<AcceptedGrantedOrgDataResponseModel>(
                It.IsAny<string>(),
                It.IsAny<SqlParameter[]>()),
            Times.Once);
    }

    [TestMethod]
    public async Task GetOrganisationsDetails_ValidRequestWithOneSubsidiary_ReturnsOrganisationDetails()
    {
        // Arrange
        var expectedData = new List<AcceptedGrantedOrgDataResponseModel>
        {
            new ()
            {
                OrganisationId = 12345,
                SubsidiaryId = null,
                OrganisationName = "Example Organisation Ltd",
                TradingName = "Example Trading Name",
                CompaniesHouseNumber = "12345678",
                HomeNationCode = "ENG",
                ServiceOfNoticeAddrLine1 = "123 Example Street",
                ServiceOfNoticeAddrLine2 = "Suite 456",
                ServiceOfNoticeAddrCity = "London",
                ServiceOfNoticeAddrCounty = "Greater London",
                ServiceOfNoticeAddrCountry = "United Kingdom",
                ServiceOfNoticeAddrPostcode = "E1 6AN",
                ServiceOfNoticeAddrPhoneNumber = "+44 20 7946 0123",
                SoleTraderFirstName = "John",
                SoleTraderLastName = "Doe",
                SoleTraderPhoneNumber = "+44 7911 123456",
                SoleTraderEmail = "john.doe@example.com",
                PrimaryContactPersonFirstName = "Jane",
                PrimaryContactPersonLastName = "Smith",
                PrimaryContactPersonPhoneNumber = "+44 7911 654321",
                PrimaryContactPersonEmail = "jane.smith@example.com",
            },
            new ()
            {
                OrganisationId = 12345,
                SubsidiaryId = "900001",
                OrganisationName = "Happy Shopper",
                TradingName = "Subsidiary Trading Name",
            },
        };

        _mockSynapseDbContext
            .Setup(ctx => ctx.RunSqlAsync<AcceptedGrantedOrgDataResponseModel>(It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
            .ReturnsAsync(expectedData);

        // Act
        var result = await _organisationService.GetOrganisationsDetails();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);

        var firstOrganisation = result.First();

        Assert.AreEqual("12345", firstOrganisation.OrganisationId);
        Assert.AreEqual("Example Organisation Ltd", firstOrganisation.OrganisationName);
        Assert.AreEqual("Example Trading Name", firstOrganisation.OrganisationTradingName);
        Assert.AreEqual("12345678", firstOrganisation.CompaniesHouseNumber);
        Assert.AreEqual("ENG", firstOrganisation.HomeNationCode);
        Assert.AreEqual("123 Example Street", firstOrganisation.ServiceOfNoticeAddrLine1);
        Assert.AreEqual("Suite 456", firstOrganisation.ServiceOfNoticeAddrLine2);
        Assert.AreEqual("London", firstOrganisation.ServiceOfNoticeAddrCity);
        Assert.AreEqual("Greater London", firstOrganisation.ServiceOfNoticeAddrCounty);
        Assert.AreEqual("United Kingdom", firstOrganisation.ServiceOfNoticeAddrCountry);
        Assert.AreEqual("E1 6AN", firstOrganisation.ServiceOfNoticeAddrPostcode);
        Assert.AreEqual("+44 20 7946 0123", firstOrganisation.ServiceOfNoticeAddrPhoneNumber);
        Assert.AreEqual("John", firstOrganisation.SoleTraderFirstName);
        Assert.AreEqual("Doe", firstOrganisation.SoleTraderLastName);
        Assert.AreEqual("+44 7911 123456", firstOrganisation.SoleTraderPhoneNumber);
        Assert.AreEqual("john.doe@example.com", firstOrganisation.SoleTraderEmail);
        Assert.AreEqual("Jane", firstOrganisation.PrimaryContactPersonFirstName);
        Assert.AreEqual("Smith", firstOrganisation.PrimaryContactPersonLastName);
        Assert.AreEqual("+44 7911 654321", firstOrganisation.PrimaryContactPersonPhoneNumber);
        Assert.AreEqual("jane.smith@example.com", firstOrganisation.PrimaryContactPersonEmail);

        Assert.IsNotNull(firstOrganisation.SubsidiaryDetails);
        Assert.AreEqual(1, firstOrganisation.SubsidiaryDetails.Count);
        Assert.IsNotNull(firstOrganisation.SubsidiaryDetails);

        Assert.AreEqual("900001", firstOrganisation.SubsidiaryDetails[0].SubsidiaryId);
        Assert.AreEqual("Happy Shopper", firstOrganisation.SubsidiaryDetails[0].SubsidiaryName);
        Assert.AreEqual("Subsidiary Trading Name", firstOrganisation.SubsidiaryDetails[0].SubsidiaryTradingName);

        _mockSynapseDbContext.Verify(
            ctx => ctx.RunSqlAsync<AcceptedGrantedOrgDataResponseModel>(
                It.IsAny<string>(),
                It.IsAny<SqlParameter[]>()),
            Times.Once);
    }

    [TestMethod]
    public async Task GetOrganisationsDetails_ExceptionThrown_OnError()
    {
        // Arrange
        _mockSynapseDbContext
            .Setup(ctx => ctx.RunSqlAsync<AcceptedGrantedOrgDataResponseModel>(It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<Exception>(() => _organisationService.GetOrganisationsDetails(It.IsAny<string>()));

        // Assert
        Assert.AreEqual("Database error", exception.Message);
    }
}