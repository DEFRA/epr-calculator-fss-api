using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace EPR.Calculator.FSS.API.Models;

[ExcludeFromCodeCoverage]
public record OrganisationSearchFilter
{
    public required string? ApprovedAfter { get; init; }

    public required string? FinancialYear { get; init; }
}
