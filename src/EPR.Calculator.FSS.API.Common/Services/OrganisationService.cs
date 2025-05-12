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
        var orgList = new List<OrganisationDetails>();

        try
        {
            const string sql = "EXECUTE [dbo].[GetLatestAcceptedGrantedOrgData] @createdOrModifiedAfter";

            var parameters = new[]
            {
                new SqlParameter("@createdOrModifiedAfter", SqlDbType.NVarChar) { Value = createdOrModifiedAfter },
            };

            var dbResponse = await _synapseDbContext.RunSqlAsync<AcceptedGrantedOrgDataResponseModel>(sql, parameters);

            var list = dbResponse
                ?.Select(x => new OrganisationDetails
                {
                    OrganisationId = x.OrganisationId?.ToString() ?? string.Empty,
                    OrganisationName = x.OrganisationName,
                    OrganisationTradingName = x.TradingName,
                    CompaniesHouseNumber = x.CompaniesHouseNumber,
                    HomeNationCode = x.HomeNationCode,
                    ServiceOfNoticeAddrLine1 = x.ServiceOfNoticeAddrLine1,
                    ServiceOfNoticeAddrLine2 = x.ServiceOfNoticeAddrLine2,
                    ServiceOfNoticeAddrCity = x.ServiceOfNoticeAddrCity,
                    ServiceOfNoticeAddrCounty = x.ServiceOfNoticeAddrCounty,
                    ServiceOfNoticeAddrCountry = x.ServiceOfNoticeAddrCountry,
                    ServiceOfNoticeAddrPostcode = x.ServiceOfNoticeAddrPostcode,
                    ServiceOfNoticeAddrPhoneNumber = x.ServiceOfNoticeAddrPhoneNumber,
                    SoleTraderFirstName = x.SoleTraderFirstName,
                    SoleTraderLastName = x.SoleTraderLastName,
                    SoleTraderPhoneNumber = x.SoleTraderPhoneNumber,
                    SoleTraderEmail = x.SoleTraderEmail,
                    PrimaryContactPersonFirstName = x.PrimaryContactPersonFirstName,
                    PrimaryContactPersonLastName = x.PrimaryContactPersonLastName,
                    PrimaryContactPersonPhoneNumber = x.PrimaryContactPersonPhoneNumber,
                    PrimaryContactPersonEmail = x.PrimaryContactPersonEmail,
                    //SubsidiaryDetails = x.SubsidiaryDetails
                })
                .ToList()
                ?? new List<OrganisationDetails>();
            return list.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in GetOrganisationsDetails method. From: {CreatedOrModifiedAfter}", createdOrModifiedAfter);

            throw;
        }

        return orgList;
    }
}