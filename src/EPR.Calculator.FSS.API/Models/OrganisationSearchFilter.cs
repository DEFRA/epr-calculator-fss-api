using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.FSS.API.Models;

[ExcludeFromCodeCoverage]
public record OrganisationSearchFilter
{
    public required string? CreatedOrModifiedAfter { get; init; }

    public required string? FinancialYear { get; init; }
}
