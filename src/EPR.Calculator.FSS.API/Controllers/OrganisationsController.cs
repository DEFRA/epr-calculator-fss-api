namespace EPR.Calculator.FSS.API.Controllers;

using EPR.Calculator.FSS.API.Common.Models;
using EPR.Calculator.FSS.API.Common.Services;
using EPR.Calculator.FSS.API.Shared;
using EPR.Calculator.FSS.API.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

/// <summary>
/// Controller for the API to retrieve Organization Details.
/// </summary>
[ApiController]
[Route("v1/organisations-details")]
public class OrganisationsController : ControllerBase
{
    private readonly IOrganisationService _organisationService;
    private readonly ILogger<OrganisationsController> _logger;
    private OrganisationSearchFilterValidator _organisationSearchFilterValidator;

    public OrganisationsController(
        IOrganisationService organisationService,
        OrganisationSearchFilterValidator organisationSearchFilterValidator,
        ILogger<OrganisationsController> logger)
    {
        this._organisationService = organisationService;
        this._logger = logger;
        this._organisationSearchFilterValidator = organisationSearchFilterValidator;
    }

    [HttpGet]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrganisationsDetails([FromQuery] string? createdOrModifiedAfter)
    {
        if (createdOrModifiedAfter != null)
        {
            // Validator for createdOrModifiedAfter - should be a valid date when not null.
            // This is an optional date parameter in ISO 8601 format(YYYY - MM - DD). If no date is passed, all organization records are returned.
            // If a date is passed, only records created or modified on or after that date are returned.
            OrganisationSearchFilter? orgSearch = new OrganisationSearchFilter();
            orgSearch = new OrganisationSearchFilter { CreatedOrModifiedAfter = createdOrModifiedAfter };
            var result = this._organisationSearchFilterValidator.Validate(orgSearch);
            if (!result.IsValid)
            {
                return HandleError.HandleErrorWithStatusCode(HttpStatusCode.BadRequest);
            }
        }

        try
        {
            var organisationList = await _organisationService.GetOrganisationsDetails(createdOrModifiedAfter);
            if (organisationList == null)
            {
                return HandleError.HandleErrorWithStatusCode(System.Net.HttpStatusCode.BadRequest);
            }

            if (organisationList.Count > 0)
            {
                return Ok(new OrganisationsDetailsResponse { OrganisationsDetails = [.. organisationList] });
            }
            else
            {
                // TODO: Confirm what this should return - possibly 404
                return NoContent();
            }
        }
        catch (Exception e)
        {
            this._logger.LogError(e, "Error Getting the Organisation details");
            return HandleError.Handle(e);
        }
    }
}