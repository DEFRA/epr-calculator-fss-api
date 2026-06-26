using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.FSS.API.Models;

[ExcludeFromCodeCoverage]
public class OrganisationSearchFilter
{
    public required string? CreatedOrModifiedAfter { get; set; }

    public required string? FinancialYear { get; set; }
}
