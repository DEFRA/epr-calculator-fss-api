using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.FSS.API.Common.Models;

[ExcludeFromCodeCoverage]
public class OrganisationResponseModel : OrganisationModel
{
    public DateTimeOffset CreatedOn { get; set; }

    public int Id { get; set; }
}
