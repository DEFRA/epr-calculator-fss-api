using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.FSS.API.Models;

[ExcludeFromCodeCoverage]
public class OrganisationSearchFilter
{
    public string? CreatedOrModifiedAfter { get; set; }

    public string? FinancialYear { get; set; }
}