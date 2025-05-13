namespace EPR.Calculator.FSS.API.Controllers;

using EPR.Calculator.FSS.API.Common.Models;
using EPR.Calculator.FSS.API.Common.Services;
using EPR.Calculator.FSS.API.Common.Validators;
using EPR.Calculator.FSS.API.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using System.Reflection.Metadata;

/// <summary>
/// Controller for the API to retrieve Organization Details.
/// </summary>
[ApiController]
[Route("organisations-details")]
public class OrganisationsController : ControllerBase
{
    private readonly IOrganisationService _organisationService;
    private readonly ILogger<OrganisationsController> _logger;
    private AbstractValidator<OrganisationSearchFilter> _organisationSearchFilterValidator;

    public OrganisationsController(
        IOrganisationService organisationService,
        AbstractValidator<OrganisationSearchFilter> organisationSearchFilterValidator,
        ILogger<OrganisationsController> logger)
    {
        _organisationService = organisationService;
        _logger = logger;
        _organisationSearchFilterValidator = organisationSearchFilterValidator;
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
            // validator for createdOrModifiedAfter - should be a valid date when not null
            // Optional date parameter in ISO 8601 format(YYYY - MM - DD).If no date is passed, all organization records are returned.
            // If a date is passed, only records created or modified on or after that date are returned.
            OrganisationSearchFilter? orgSerach = new OrganisationSearchFilter();
            orgSerach = new OrganisationSearchFilter() { CreatedOrModifiedAfter = createdOrModifiedAfter };
            var result = this._organisationSearchFilterValidator.Validate(orgSerach);
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