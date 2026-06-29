namespace EPR.Calculator.FSS.API.Models;

public record OrganisationDetails
{
    public required string OrganisationId { get; init; }

    public required string? OrganisationName { get; init; }

    public required string? OrganisationTradingName { get; init; }

    public required string FinancialYear {get; init; }

    public required string? CompaniesHouseNumber { get; init; }

    public required string? HomeNationCode { get; init; }

    public required string? ServiceOfNoticeAddrLine1 { get; init; }

    public required string? ServiceOfNoticeAddrLine2 { get; init; }

    public required string? ServiceOfNoticeAddrCity { get; init; }

    public required string? ServiceOfNoticeAddrCounty { get; init; }

    public required string? ServiceOfNoticeAddrCountry { get; init; }

    public required string? ServiceOfNoticeAddrPostcode { get; init; }

    public required string? ServiceOfNoticeAddrPhoneNumber { get; init; }

    public required string? SoleTraderFirstName { get; init; }

    public required string? SoleTraderLastName { get; init; }

    public required string? SoleTraderPhoneNumber { get; init; }

    public required string? SoleTraderEmail { get; init; }

    public required string? PrimaryContactPersonFirstName { get; init; }

    public required string? PrimaryContactPersonLastName { get; init; }

    public required string? PrimaryContactPersonPhoneNumber { get; init; }

    public required string? PrimaryContactPersonEmail { get; init; }

    public List<SubsidiaryDetails> SubsidiaryDetails { get; init; } = [];
}
