using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.FSS.API.Common.Models;

[ExcludeFromCodeCoverage]
public class SubsidiaryDetails
{
    required public string SubsidiaryId { get; set; }

    public string? SubsidiaryName { get; set; }

    public string? SubsidiaryTradingName { get; set; }
}