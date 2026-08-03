using System.Data;
using System.Globalization;
using EPR.Calculator.FSS.API.Data;
using EPR.Calculator.FSS.API.Data.Entities;
using EPR.Calculator.FSS.API.Models;
using Microsoft.Data.SqlClient;

namespace EPR.Calculator.FSS.API.Services;

public interface IOrganisationService
{

     /// <summary>
    /// Get the Organisation Data for the calculator run.
    /// </summary>
    /// <param name="approvedAfter">Date the data was created or last changed.</param>
    /// <param name="relativeYear">the relative year for the data.</param>
    /// <param name="cancellationToken">The database cancellation token .</param>
    /// <returns>Organisation details collection.</returns>
    Task<IReadOnlyCollection<OrganisationDetails>> GetOrganisationsDetails(
        DateTime? approvedAfter,
        RelativeYear? relativeYear,
        CancellationToken cancellationToken);
}

#pragma warning disable CA1848 // Use the LoggerMessage delegates
public class OrganisationService(
    SynapseDbContext synapseDbContext,
    ILogger<OrganisationService> logger)
    : IOrganisationService
{
    public async Task<IReadOnlyCollection<OrganisationDetails>> GetOrganisationsDetails(
        DateTime? approvedAfter,
        RelativeYear? relativeYear,
        CancellationToken cancellationToken)
    {
        var organisationsList = new List<OrganisationDetails>();

        const string sql = "EXECUTE [dbo].[GetLatestAcceptedGrantedOrgData] @approvedAfter, @relativeYear";

        var parameters = new[]
        {
            new SqlParameter("@approvedAfter", SqlDbType.DateTime) { Value = approvedAfter },
            new SqlParameter("@relativeYear" , SqlDbType.Int     ) { Value = relativeYear?.Value },
        };

        var acceptedGrantedOrgDataResponse = await synapseDbContext
            .RunSqlAsync<AcceptedGrantedOrgDataResponseModel>(sql, cancellationToken, parameters);

        var organisationsLookup = acceptedGrantedOrgDataResponse
            .Where(x => x.OrganisationId is not null)
            .Select(x => new
            {
                Data = x,
                SubsidiaryId = string.IsNullOrWhiteSpace(x.SubsidiaryId)
                    ? null
                    : x.SubsidiaryId,
            })
            .ToLookup(x => x.Data.OrganisationId!.Value); // Add relative year to group by when agreed with FSS - currently dedupes by OrganisationId && DecisionDate to preserve approvedAfter existing functionality.

        foreach (var organisationKey in organisationsLookup.Select(x => x.Key))
        {
            var organisationRecords = organisationsLookup[organisationKey];

            var parent = organisationRecords
                .Where(x => x.SubsidiaryId is null)
                .OrderByDescending(x => x.Data.DecisionDateTime)
                .Select(x => x.Data)
                .FirstOrDefault();

            if (parent is null)
            {
                logger.LogWarning("Parent organisation not found for organisation_id {OrganisationKey}. Skipping.", organisationKey);
                continue;
            }

            var subsidiaries = organisationRecords
                .Where(x => !string.IsNullOrWhiteSpace(x.SubsidiaryId) && x.Data.RelativeYear == parent.RelativeYear)
                .Select(x => x.Data)
                .Select(x => new SubsidiaryDetails
                {
                    SubsidiaryId = x.SubsidiaryId!,
                    SubsidiaryName = x.OrganisationName,
                    SubsidiaryTradingName = x.TradingName,
                    FinancialYear = ToFinancialYear(x.RelativeYear),
                    ApprovedDate =  x.DecisionDateTime
                })
                .ToList();

            organisationsList.Add(new OrganisationDetails
            {
                OrganisationId = organisationKey.ToString(CultureInfo.InvariantCulture),
                FinancialYear = ToFinancialYear(parent.RelativeYear),
                ApprovedDate = parent.DecisionDateTime,

                OrganisationName = parent.OrganisationName,
                OrganisationTradingName = parent.TradingName,
                CompaniesHouseNumber = parent.CompaniesHouseNumber,
                HomeNationCode = parent.HomeNationCode,

                ServiceOfNoticeAddrLine1 = parent.ServiceOfNoticeAddrLine1,
                ServiceOfNoticeAddrLine2 = parent.ServiceOfNoticeAddrLine2,
                ServiceOfNoticeAddrCity = parent.ServiceOfNoticeAddrCity,
                ServiceOfNoticeAddrCounty = parent.ServiceOfNoticeAddrCounty,
                ServiceOfNoticeAddrCountry = parent.ServiceOfNoticeAddrCountry,
                ServiceOfNoticeAddrPostcode = parent.ServiceOfNoticeAddrPostcode,
                ServiceOfNoticeAddrPhoneNumber = parent.ServiceOfNoticeAddrPhoneNumber,

                SoleTraderFirstName = parent.SoleTraderFirstName,
                SoleTraderLastName = parent.SoleTraderLastName,
                SoleTraderPhoneNumber = parent.SoleTraderPhoneNumber,
                SoleTraderEmail = parent.SoleTraderEmail,

                PrimaryContactPersonFirstName = parent.PrimaryContactPersonFirstName,
                PrimaryContactPersonLastName = parent.PrimaryContactPersonLastName,
                PrimaryContactPersonPhoneNumber = parent.PrimaryContactPersonPhoneNumber,
                PrimaryContactPersonEmail = parent.PrimaryContactPersonEmail,

                SubsidiaryDetails = subsidiaries,
            });
        }

        return organisationsList
            .OrderBy(x => long.Parse(x.OrganisationId, CultureInfo.InvariantCulture))
            .ThenBy(x => x.FinancialYear)
            .ToList()
            .AsReadOnly();
    }

    private static string ToFinancialYear(int relativeYear) =>
        $"{relativeYear}-{(relativeYear + 1) % 100:D2}";

}
