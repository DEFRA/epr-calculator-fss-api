using EPR.Calculator.FSS.API.Models;

namespace EPR.Calculator.FSS.API.Services;

public interface IOrganisationService
{
    /// <summary>
    /// Get the Organisation Data for the calculator run.
    /// </summary>
    /// <param name="cancellationToken">The database cancellation token .</param>
    /// <param name="createdOrModifiedAfter">Date the data was created or last changed.</param>
    /// <param name="relativeYear">the relative year for the data.</param>
    /// <returns>Organisation details collection.</returns>
    Task<IReadOnlyCollection<OrganisationDetails>> GetOrganisationsDetails(
        CancellationToken cancellationToken,
        string? createdOrModifiedAfter = null,
        int? relativeYear = null);
}
