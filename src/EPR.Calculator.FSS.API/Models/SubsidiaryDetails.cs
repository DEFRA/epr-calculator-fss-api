using Microsoft.VisualBasic;

namespace EPR.Calculator.FSS.API.Models;

public record SubsidiaryDetails
{
    public required string SubsidiaryId { get; init; }

    public required string? SubsidiaryName { get; init; }

    public required string? SubsidiaryTradingName { get; init; }

    public required string FinancialYear { get; init; }
}
