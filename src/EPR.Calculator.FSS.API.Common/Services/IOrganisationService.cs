namespace EPR.Calculator.FSS.API.Common.Services;

using EPR.Calculator.FSS.API.Common.Models;
public interface IOrganisationService
{
    /// <summary>
    /// Get the Organisation Data for the calculator run.
    /// </summary>
    /// <param name="createdOrModifiedAfter">Date the data was created or last changed.</param>
    /// <returns>Organisation details collection.</returns>
    Task<IReadOnlyCollection<OrganisationDetails>> GetOrganisationsDetails(string? createdOrModifiedAfter = null);
}
