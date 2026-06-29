namespace EPR.Calculator.FSS.API.Models;

public record    OrganisationsDetailsResponse
{
    public required IList<OrganisationDetails> OrganisationsDetails { get; init; }
}
