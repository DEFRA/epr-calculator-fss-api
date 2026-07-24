using System.ComponentModel.DataAnnotations.Schema;

namespace EPR.Calculator.FSS.API.Data.Entities;

public record AcceptedGrantedOrgDataResponseModel
{
    [Column("organisation_id")]
    public required int? OrganisationId { get; init; }

    [Column("subsidiary_id")]
    public required string? SubsidiaryId { get; init; }

    [Column("organisation_name")]
    public required string? OrganisationName { get; init; }

    [Column("trading_name")]
    public required string? TradingName { get; init; }

    [Column("relative_year")]
    public required int RelativeYear { get; init; }

    [Column("companies_house_number")]
    public required string? CompaniesHouseNumber { get; init; }

    [Column("home_nation_code")]
    public required string? HomeNationCode { get; init; }

    [Column("service_of_notice_addr_line1")]
    public required string? ServiceOfNoticeAddrLine1 { get; init; }

    [Column("service_of_notice_addr_line2")]
    public required string? ServiceOfNoticeAddrLine2 { get; init; }

    [Column("service_of_notice_addr_city")]
    public required string? ServiceOfNoticeAddrCity { get; init; }

    [Column("service_of_notice_addr_county")]
    public required string? ServiceOfNoticeAddrCounty { get; init; }

    [Column("service_of_notice_addr_country")]
    public required string? ServiceOfNoticeAddrCountry { get; init; }

    [Column("service_of_notice_addr_postcode")]
    public required string? ServiceOfNoticeAddrPostcode { get; init; }

    [Column("service_of_notice_addr_phone_number")]
    public required string? ServiceOfNoticeAddrPhoneNumber { get; init; }

    [Column("sole_trader_first_name")]
    public required string? SoleTraderFirstName { get; init; }

    [Column("sole_trader_last_name")]
    public required string? SoleTraderLastName { get; init; }

    [Column("sole_trader_phone_number")]
    public required string? SoleTraderPhoneNumber { get; init; }

    [Column("sole_trader_email")]
    public required string? SoleTraderEmail { get; init; }

    [Column("primary_contact_person_first_name")]
    public required string? PrimaryContactPersonFirstName { get; init; }

    [Column("primary_contact_person_last_name")]
    public required string? PrimaryContactPersonLastName { get; init; }

    [Column("primary_contact_person_phone_number")]
    public required string? PrimaryContactPersonPhoneNumber { get; init; }

    [Column("primary_contact_person_email")]
    public required string? PrimaryContactPersonEmail { get; init; }

    [Column("Decision_Date")]
    public required string DecisionDate { get; init; } // Used to dedupe organisation across relative years
}
