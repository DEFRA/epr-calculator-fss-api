namespace EPR.Calculator.FSS.API.Common.UnitTests.Services;

[TestClass]
public class OrganisationServiceTests
{
    private readonly OrganisationService _organisationService;

    public OrganisationServiceTests()
    {
        _organisationService = new OrganisationService();
    }

    [TestMethod]
    public async Task GetOrganisationsDetails_ReturnsEmptyResult_WhenNoData()
    {
        // Arrange
        // Act
        var result = await _organisationService.GetOrganisationsDetails();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }
}