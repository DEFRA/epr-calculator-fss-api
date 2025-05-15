namespace EPR.Calculator.FSS.API.Controllers;

using EPR.Calculator.FSS.API.Common.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("organisations-details")]
public class OrganisationsController : ControllerBase
{
    private readonly IOrganisationService _organisationService;
    private readonly ILogger<OrganisationsController> _logger;

    public OrganisationsController(IOrganisationService organisationService, ILogger<OrganisationsController> logger)
    {
        _organisationService = organisationService;
        _logger = logger;
    }

    [HttpGet]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrganisationsDetails([FromQuery]string? createdOrModifiedAfter)
    {
        // TODO: Add validator for createdOrModifiedAfter - should be a valid date when not null

        var organisationList = await _organisationService.GetOrganisationsDetails(createdOrModifiedAfter);
        if (organisationList.Count > 0)
        {
            return Ok(organisationList);
        }
        else
        {
            // TODO: Confirm what this should return - possibly 404
            return NoContent();
        }
    }
}