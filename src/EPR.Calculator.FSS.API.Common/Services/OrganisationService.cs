namespace EPR.Calculator.FSS.API;

using EPR.Calculator.FSS.API.Common.Data;
using EPR.Calculator.FSS.API.Common.Data.Entities;
using EPR.Calculator.FSS.API.Common.Models;
using EPR.Calculator.FSS.API.Common.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

#pragma warning disable CA1848 // Use the LoggerMessage delegates

public class OrganisationService(
    SynapseDbContext synapseDbContext,
    ILogger<OrganisationService> logger)
    : IOrganisationService
{
    public async Task<IReadOnlyCollection<OrganisationDetails>> GetOrganisationsDetails(string? createdOrModifiedAfter = null)
    {
        var organisationsList = new List<OrganisationDetails>();

        try
        {
            const string sql = "EXECUTE [dbo].[GetLatestAcceptedGrantedOrgData] @createdOrModifiedAfter";

            var parameters = new[]
            {
                new SqlParameter("@createdOrModifiedAfter", SqlDbType.NVarChar) { Value = createdOrModifiedAfter },
            };

            var acceptedGrantedOrgDataResponse = await synapseDbContext.RunSqlAsync<AcceptedGrantedOrgDataResponseModel>(sql, parameters);

            var organisationsLookup = acceptedGrantedOrgDataResponse
               .Where(x => x.OrganisationId is not null)
               .ToLookup(x => x.OrganisationId!.Value);

            foreach (var organisationId in organisationsLookup.Select(x => x.Key))
            {
                var parent = organisationsLookup[organisationId]
                    .FirstOrDefault(o => string.IsNullOrWhiteSpace(o.SubsidiaryId));

                if (parent is null)
                {
                    logger.LogWarning("Parent organisation not found for organisation_id {OrganisationId}. Skipping this organisation.", organisationId);
                    continue;
                }

                organisationsList.Add(new OrganisationDetails
                {
                    OrganisationId = organisationId.ToString(CultureInfo.InvariantCulture),
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
                    SubsidiaryDetails = organisationsLookup[organisationId]
                        .Where(x => x.SubsidiaryId is not null)
                        .Select(s => new SubsidiaryDetails
                        {
                            SubsidiaryId = s.SubsidiaryId!,
                            SubsidiaryName = s.OrganisationName,
                            SubsidiaryTradingName = s.TradingName,
                        })
                        .ToList(),
                });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred in GetOrganisationsDetails method. From: {CreatedOrModifiedAfter}", createdOrModifiedAfter);
            throw;
        }

        return organisationsList.AsReadOnly();
    }
}