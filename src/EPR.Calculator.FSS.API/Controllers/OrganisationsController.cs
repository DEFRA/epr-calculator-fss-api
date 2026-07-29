using EPR.Calculator.FSS.API.Helpers;
using EPR.Calculator.FSS.API.Models;
using EPR.Calculator.FSS.API.Services;
using EPR.Calculator.FSS.API.Validators;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.FSS.API.Controllers;

/// <summary>
/// Controller for the API to retrieve Organization Details.
/// </summary>
[ApiController]
[Route("api/v1")]
public class OrganisationsController(
    IOrganisationService organisationService,
    OrganisationSearchFilterValidator organisationSearchFilterValidator,
    ILogger<OrganisationsController> logger)
    : ControllerBase
{
    private const string ErrorMessage = "Error Getting the Organisation details";

    [HttpGet]
    [Route("organisations-details")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrganisationsDetails([FromQuery] string? approvedAfter, [FromQuery] string? financialYear)
    {
        // approvedAfter is an optional date parameter in ISO 8601 format(YYYY - MM - DD)
        // financialYear is an optional date parameter in format YYYY-YY
        var validation = organisationSearchFilterValidator.Validate(new OrganisationSearchFilter
        {
            ApprovedAfter = approvedAfter,
            FinancialYear = financialYear,
        });

        if (!validation.IsValid)
        {
            return BadRequest(new ApiError
            {
                Error       = "Bad Request",
                Message     = $"The request was malformed or invalid - {string.Join(", ",  validation.Errors)}",
                StatusCode  = 400,
                ErrorCode   = "invalid_request",
                Description = "The request did not conform to the required format."
            });
        }

        try
        {
            var organisationList = await organisationService.GetOrganisationsDetails(
                cancellationToken: HttpContext.RequestAborted,
                approvedAfter    : OrganisationSearchFilterValidator.TryParseApprovedAfter(approvedAfter),
                relativeYear     : OrganisationSearchFilterValidator.TryParseFinancialYear(financialYear));

            if (organisationList.Count > 0)
            {
                return Ok(new OrganisationsDetailsResponse { OrganisationsDetails = organisationList.ToList() });
            }
            else
            {
                return NotFound(new ApiError
                {
                    Error       = "Not Found",
                    Message     = "The requested resource could not be found.",
                    StatusCode  = 404,
                    ErrorCode   = "resource_not_found",
                    Description = "The resource you requested does not exist."
                });
            }
        }
        catch (Exception e)
        {
            logger.LogErrorMessage(ErrorMessage, e);
            return HandleError.Handle(e);
        }
    }
}
