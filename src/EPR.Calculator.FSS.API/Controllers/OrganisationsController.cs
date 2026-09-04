using EPR.Calculator.FSS.API.Helpers;
using EPR.Calculator.FSS.API.Models;
using EPR.Calculator.FSS.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.FSS.API.Controllers;

/// <summary>
/// Controller for the API to retrieve Organization Details.
/// </summary>
[ApiController]
[Route("api/v1")]
public class OrganisationsController(
    IOrganisationService organisationService)
    : ControllerBase
{
    [HttpGet("organisations-details")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrganisationsDetails(
        [FromQuery] DateTimeOffset? approvedAfter,
        [FromQuery] FinancialYear? financialYear)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiError
            {
                Error       = "Bad Request",
                Message     = $"The request was malformed or invalid - {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}",
                StatusCode  = 400,
                ErrorCode   = "invalid_request",
                Description = "The request did not conform to the required format."
            });
        }

        return Ok(new OrganisationsDetailsResponse
        {
            OrganisationsDetails = await organisationService.GetOrganisationsDetails(
                approvedAfter    : approvedAfter?.UtcDateTime,
                relativeYear     : financialYear?.ToRelativeYear(),
                cancellationToken: HttpContext.RequestAborted)
        });
    }
}
