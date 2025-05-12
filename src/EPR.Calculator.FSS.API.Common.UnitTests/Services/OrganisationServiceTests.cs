using Castle.Core.Logging;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.FSS.API.Common.Data;
using EPR.Calculator.FSS.API.Common.Data.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Calculator.FSS.API.Common.UnitTests.Services;

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
}
