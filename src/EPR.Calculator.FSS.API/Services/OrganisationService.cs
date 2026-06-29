using System.Data;
using System.Globalization;
using EPR.Calculator.FSS.API.Data;
using EPR.Calculator.FSS.API.Data.Entities;
using EPR.Calculator.FSS.API.Models;
using EPR.Calculator.FSS.API.Services;
using Microsoft.Data.SqlClient;

namespace EPR.Calculator.FSS.API;

#pragma warning disable CA1848 // Use the LoggerMessage delegates
public class OrganisationService(
    SynapseDbContext synapseDbContext,
    ILogger<OrganisationService> logger)
    : IOrganisationService
{

    public async Task<IReadOnlyCollection<OrganisationDetails>> GetOrganisationsDetails(
        CancellationToken cancellationToken,
        string? createdOrModifiedAfter = null,
        int? relativeYear = null)
    {
        var organisationsList = new List<OrganisationDetails>();

        const string sql = "EXECUTE [dbo].[GetLatestAcceptedGrantedOrgData] @createdOrModifiedAfter, @relativeYear";

        var parameters = new[]
        {
            new SqlParameter("@createdOrModifiedAfter", SqlDbType.NVarChar) { Value = createdOrModifiedAfter },
            new SqlParameter("@relativeYear", SqlDbType.NVarChar) { Value = relativeYear },
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
            .ToLookup(x => new
            {
                OrganisationId = x.Data.OrganisationId!.Value,
                x.Data.RelativeYear,
            });

        foreach (var organisationKey in organisationsLookup.Select(x => x.Key))
        {
            var organisationRecords = organisationsLookup[organisationKey];

            var parent = organisationRecords
                .Where(x => x.SubsidiaryId is null)
                .OrderByDescending(x => DateTimeOffset.Parse(x.Data.DecisionDate, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind))
                .Select(x => x.Data)
                .FirstOrDefault();

            if (parent is null)
            {
                logger.LogWarning(
                    "Parent organisation not found for organisation_id {OrganisationId} and relative_year {RelativeYear}. Skipping.",
                    organisationKey.OrganisationId,
                    organisationKey.RelativeYear);

                continue;
            }

            var subsidiaries = organisationRecords
                .Where(x => !string.IsNullOrWhiteSpace(x.SubsidiaryId))
                .Select(x => x.Data)
                .Select(x => new SubsidiaryDetails
                {
                    SubsidiaryId = x.SubsidiaryId!,
                    SubsidiaryName = x.OrganisationName,
                    SubsidiaryTradingName = x.TradingName,
                    FinancialYear = ToFinancialYear(x.RelativeYear),
                })
                .ToList();

            organisationsList.Add(new OrganisationDetails
            {
                OrganisationId = organisationKey.OrganisationId.ToString(CultureInfo.InvariantCulture),
                FinancialYear = ToFinancialYear(parent.RelativeYear),
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
