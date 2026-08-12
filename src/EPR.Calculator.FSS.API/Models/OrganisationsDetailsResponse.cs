namespace EPR.Calculator.FSS.API.Models;

public record    OrganisationsDetailsResponse
{
    public required IEnumerable<OrganisationDetails> OrganisationsDetails { get; init; }
}
