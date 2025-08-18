using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.FSS.API.Common.Models;

public class OrganisationDetails
{
    public string OrganisationId { get; set; }

    public string? OrganisationName { get; set; }

    public string? OrganisationTradingName { get; set; }

    public string? CompaniesHouseNumber { get; set; }

    public string? HomeNationCode { get; set; }

    public string? ServiceOfNoticeAddrLine1 { get; set; }

    public string? ServiceOfNoticeAddrLine2 { get; set; }

    public string? ServiceOfNoticeAddrCity { get; set; }

    public string? ServiceOfNoticeAddrCounty { get; set; }

    public string? ServiceOfNoticeAddrCountry { get; set; }

    public string? ServiceOfNoticeAddrPostcode { get; set; }

    public string? ServiceOfNoticeAddrPhoneNumber { get; set; }

    public string? SoleTraderFirstName { get; set; }

    public string? SoleTraderLastName { get; set; }

    public string? SoleTraderPhoneNumber { get; set; }

    public string? SoleTraderEmail { get; set; }

    public string? PrimaryContactPersonFirstName { get; set; }

    public string? PrimaryContactPersonLastName { get; set; }

    public string? PrimaryContactPersonPhoneNumber { get; set; }

    public string? PrimaryContactPersonEmail { get; set; }

    public List<SubsidiaryDetails> SubsidiaryDetails { get; set; } = new List<SubsidiaryDetails>();
}