namespace EPR.Calculator.FSS.API;

using EPR.Calculator.API.Data;
using EPR.Calculator.FSS.API.Common.Models;
using EPR.Calculator.FSS.API.Common.Services;

public class OrganisationService : IOrganisationService
{
    private readonly ApplicationDBContext context;

    public OrganisationService()
    {
    }

    public async Task<IReadOnlyCollection<OrganisationDetails>> GetOrganisationsDetails(string? createdOrModifiedAfter = null)
    {
        var orgList = new List<OrganisationDetails>();
        return orgList;
    }
}