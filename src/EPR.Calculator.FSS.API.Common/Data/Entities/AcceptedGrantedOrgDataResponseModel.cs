using System.ComponentModel.DataAnnotations.Schema;

namespace EPR.Calculator.FSS.API.Common.Data.Entities;

public class AcceptedGrantedOrgDataResponseModel
{
    [Column("organisation_id")]
    public int? OrganisationId { get; set; }

    [Column("subsidiary_id")]
    public string? SubsidiaryId { get; set; }

    [Column("organisation_name")]
    public string? OrganisationName { get; set; }

    [Column("trading_name")]
    public string? TradingName { get; set; }

    [Column("companies_house_number")]
    public string? CompaniesHouseNumber { get; set; }

    [Column("home_nation_code")]
    public string? HomeNationCode { get; set; }

    [Column("service_of_notice_addr_line1")]
    public string? ServiceOfNoticeAddrLine1 { get; set; }

    [Column("service_of_notice_addr_line2")]
    public string? ServiceOfNoticeAddrLine2 { get; set; }

    [Column("service_of_notice_addr_city")]
    public string? ServiceOfNoticeAddrCity { get; set; }

    [Column("service_of_notice_addr_county")]
    public string? ServiceOfNoticeAddrCounty { get; set; }

    [Column("service_of_notice_addr_country")]
    public string? ServiceOfNoticeAddrCountry { get; set; }

    [Column("service_of_notice_addr_postcode")]
    public string? ServiceOfNoticeAddrPostcode { get; set; }

    [Column("service_of_notice_addr_phone_number")]
    public string? ServiceOfNoticeAddrPhoneNumber { get; set; }

    [Column("sole_trader_first_name")]
    public string? SoleTraderFirstName { get; set; }

    [Column("sole_trader_last_name")]
    public string? SoleTraderLastName { get; set; }

    [Column("sole_trader_phone_number")]
    public string? SoleTraderPhoneNumber { get; set; }

    [Column("sole_trader_email")]
    public string? SoleTraderEmail { get; set; }

    [Column("primary_contact_person_first_name")]
    public string? PrimaryContactPersonFirstName { get; set; }

    [Column("primary_contact_person_last_name")]
    public string? PrimaryContactPersonLastName { get; set; }

    [Column("primary_contact_person_phone_number")]
    public string? PrimaryContactPersonPhoneNumber { get; set; }

    [Column("primary_contact_person_email")]
    public string? PrimaryContactPersonEmail { get; set; }

    [Column("FileName")]
    public string? FileName { get; set; }

    [Column("ReferenceNumber")]
    public string? ReferenceNumber { get; set; }

    [Column("IsDeleted")]
    public string? IsDeleted { get; set; }

    [Column("CreatedOn")]
    public string? CreatedOn { get; set; }

    [Column("LastUpdatedOn")]
    public string? LastUpdatedOn { get; set; }

    [Column("cfm_FileId")]
    public string? CfmFileId { get; set; }

    [Column("Regulator_Status")]
    public string? RegulatorStatus { get; set; }

    [Column("load_ts")]
    public DateTime? LoadTs { get; set; }
}
