namespace EPR.Calculator.FSS.API;

using EPR.Calculator.FSS.API.Common.Data;
using EPR.Calculator.FSS.API.Common.Data.Entities;
using EPR.Calculator.FSS.API.Common.Models;
using EPR.Calculator.FSS.API.Common.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;

public class OrganisationService : IOrganisationService
{
    private readonly SynapseDbContext _synapseDbContext;
    private readonly ILogger<OrganisationService> _logger;

    public OrganisationService(SynapseDbContext synapseDbContext,
                               ILogger<OrganisationService> logger)
    {
        _synapseDbContext = synapseDbContext;
        _logger = logger;
    }

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

            var dbResponse = await _synapseDbContext.RunSqlAsync<AcceptedGrantedOrgDataResponseModel>(sql, parameters);

            var organisationsLookup = dbResponse
               .Where(x => x.OrganisationId is not null)
               .ToLookup(x => x.OrganisationId!.Value);

            foreach (var key in organisationsLookup.Select(x => x.Key))
            {
                var parent = organisationsLookup[key].First();

                var o = new OrganisationDetails
                {
                    OrganisationId = parent.OrganisationId.ToString() ?? string.Empty,
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
                    SubsidiaryDetails = organisationsLookup[key]
                        .Where(x => x.SubsidiaryId is not null)
                        .Select(s => new SubsidiaryDetails
                        {
                            SubsidiaryId = s.SubsidiaryId!,
                            SubsidiaryName = s.OrganisationName,
                            SubsidiaryTradingName = s.TradingName,
                        })
                        .ToList(),
                };

                organisationsList.Add(o);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in GetOrganisationsDetails method. From: {CreatedOrModifiedAfter}", createdOrModifiedAfter);
            throw;
        }

        return organisationsList.AsReadOnly();
    }
}