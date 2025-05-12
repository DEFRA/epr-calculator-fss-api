using EPR.Calculator.API.Data;
using EPR.Calculator.FSS.API.Common.Models;
using EPR.Calculator.FSS.API.Common.Services;
using Microsoft.EntityFrameworkCore;
namespace EPR.Calculator.FSS.API
{
    public class OrganisationService : IOrganisationService
    {
        private readonly ApplicationDBContext context;

        public OrganisationService(
            ApplicationDBContext context)
        {
            this.context = context;
        }

        public async Task<IReadOnlyCollection<OrganisationResponseModel>> GetOrganisationsDetails(string createdOrModifiedAfter)
        {
            var orgList = new List<OrganisationResponseModel>();
            return orgList;
        }
    }
}