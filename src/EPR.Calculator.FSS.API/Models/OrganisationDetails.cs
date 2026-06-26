namespace EPR.Calculator.FSS.API.Models;

public class OrganisationDetails
{
    public required string OrganisationId { get; set; }

    public required string? OrganisationName { get; set; }

    public required string? OrganisationTradingName { get; set; }

    public required string? CompaniesHouseNumber { get; set; }

    public required string? HomeNationCode { get; set; }

    public required string? ServiceOfNoticeAddrLine1 { get; set; }

    public required string? ServiceOfNoticeAddrLine2 { get; set; }

    public required string? ServiceOfNoticeAddrCity { get; set; }

    public required string? ServiceOfNoticeAddrCounty { get; set; }

    public required string? ServiceOfNoticeAddrCountry { get; set; }

    public required string? ServiceOfNoticeAddrPostcode { get; set; }

    public required string? ServiceOfNoticeAddrPhoneNumber { get; set; }

    public required string? SoleTraderFirstName { get; set; }

    public required string? SoleTraderLastName { get; set; }

    public required string? SoleTraderPhoneNumber { get; set; }

    public required string? SoleTraderEmail { get; set; }

    public required string? PrimaryContactPersonFirstName { get; set; }

    public required string? PrimaryContactPersonLastName { get; set; }

    public required string? PrimaryContactPersonPhoneNumber { get; set; }

    public required string? PrimaryContactPersonEmail { get; set; }

    public List<SubsidiaryDetails> SubsidiaryDetails { get; set; } = new List<SubsidiaryDetails>();
}
