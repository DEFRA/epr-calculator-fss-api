namespace EPR.Calculator.FSS.API;

using EPR.Calculator.FSS.API.Common.Data;
using EPR.Calculator.FSS.API.Common.Data.Entities;
using EPR.Calculator.FSS.API.Common.Models;
using EPR.Calculator.FSS.API.Common.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
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
        try
        {
            const string sql = "EXECUTE [dbo].[GetLatestAcceptedGrantedOrgData] @createdOrModifiedAfter";

            var parameters = new[]
            {
                new SqlParameter("@createdOrModifiedAfter", SqlDbType.NVarChar) { Value = createdOrModifiedAfter },
            };

            var dbResponse = await _synapseDbContext.RunSqlAsync<AcceptedGrantedOrgDataResponseModel>(sql, parameters);

            var groupedResults = dbResponse?
                .Where(x => x.OrganisationId is not null)
                .GroupBy(x => x.OrganisationId);

            var resultList = new List<OrganisationDetails>();

            if (groupedResults is not null)
            {
                foreach (var group in groupedResults)
                {
                    // Should have one item with no subsidiary
                    var items = group.OrderBy(g => g.SubsidiaryId);

                    // First one should have the parent with a null subsidiary id
                    var parent = items
                        .FirstOrDefault(x => x.SubsidiaryId is null);

                    if (parent is null)
                    {
                        _logger.LogWarning("No parent item found for organisation {OrganisationId}", group.Key);
                        continue;
                    }

                    var parentOrganisation = new OrganisationDetails
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
                        SubsidiaryDetails = new List<SubsidiaryDetails>(),
                    };

                    foreach (var item in items.Where(x => x.SubsidiaryId is not null))
                    {
                        parentOrganisation.SubsidiaryDetails.Add(new SubsidiaryDetails
                        {
                            SubsidiaryId = item.SubsidiaryId!,
                            SubsidiaryName = item.OrganisationName,
                            SubsidiaryTradingName = item.TradingName,
                        });
                    }

                    resultList.Add(parentOrganisation);
                }
            }

            return resultList.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in GetOrganisationsDetails method. From: {CreatedOrModifiedAfter}", createdOrModifiedAfter);

            throw;
        }
    }
}