using EPR.Calculator.FSS.API.Common.Models;

namespace EPR.Calculator.FSS.API.Common.Services
{
    public interface IOrganisationService
    {
        /// <summary>
        /// Get the Organisation Data for the calculator Run
        /// </summary>
        /// <param name="createdOrModifiedAfter"></param>
        /// <returns></returns>
        Task<IReadOnlyCollection<OrganisationResponseModel>> GetOrganisationsDetails(string createdOrModifiedAfter);

        /*Task<PaginatedResponse<OrganisationSearchResult>> GetOrganisationsBySearchKeyword(string query, int pageSize, int page);*/
    }
}
