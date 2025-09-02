namespace EPR.Calculator.FSS.API.Controllers;

using EPR.Calculator.FSS.API.Common.Models;
using EPR.Calculator.FSS.API.Common.Services;
using EPR.Calculator.FSS.API.Helpers;
using EPR.Calculator.FSS.API.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

/// <summary>
/// Controller for the API to retrieve Organization Details.
/// </summary>
[ApiController]
public class OrganisationsController : ControllerBase
{
    private const string ErrorMessage = "Error Getting the Organisation details";
    private readonly IOrganisationService _organisationService;
    private readonly ILogger<OrganisationsController> _logger;
    private readonly OrganisationSearchFilterValidator _organisationSearchFilterValidator;

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
    [Route("api/v1/organisations-details")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
                return BadRequest(new ApiError
                {
                    Error = "Bad Request",
                    Message = "The request was malformed or invalid.",
                    StatusCode = 400,
                    ErrorCode = "invalid_request",
                    Description = "The request did not conform to the required format."
                });
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
                return NotFound(new ApiError
                {
                    Error = "Not Found",
                    Message = "The requested resource could not be found.",
                    StatusCode = 404,
                    ErrorCode = "resource_not_found",
                    Description = "The resource you requested does not exist."
                });
            }
        }
        catch (Exception e)
        {
            this._logger.LogErrorMessage(e.Message, e);
            return HandleError.Handle(e);
        }
    }
}