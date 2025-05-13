namespace EPR.Calculator.FSS.API.Controllers;

using EPR.Calculator.FSS.API.Common.Models;
using EPR.Calculator.FSS.API.Common.Services;
using EPR.Calculator.FSS.API.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

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
    public async Task<IActionResult> GetOrganisationsDetails([FromQuery] string? createdOrModifiedAfter)
    {
        if (createdOrModifiedAfter == null)
        {
            return HandleError.HandleErrorWithStatusCode(HttpStatusCode.BadRequest);
        }

        try
        {
            //TODO: Add validator for createdOrModifiedAfter - should be a valid date when not null

            var organisationList = await _organisationService.GetOrganisationsDetails(createdOrModifiedAfter);
            if (organisationList == null)
            {
                return HandleError.HandleErrorWithStatusCode(System.Net.HttpStatusCode.BadRequest);
            }

            if (organisationList.Count > 0)
            {
                return Ok(organisationList);
            }
            else
            {
                //TODO: Confirm what this should return - possibly 404
                return NoContent();
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error Getting the Organisation details");
            return HandleError.Handle(e);
        }
    }
}