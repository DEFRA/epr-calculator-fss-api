using System.ComponentModel.DataAnnotations;

namespace EPR.Calculator.FSS.API.Models;

public record SubsidiaryDetails
{
    public required string SubsidiaryId { get; init; }

    public required string? SubsidiaryName { get; init; }

    public required string? SubsidiaryTradingName { get; init; }

    [RegularExpression(@"^[0-9]{4}-[0-9]{2}$")]
    public required string FinancialYear { get; init; }

    public required DateTime ApprovedDate { get; init; }

}
