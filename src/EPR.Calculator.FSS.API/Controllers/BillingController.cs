using EPR.Calculator.FSS.API.Helpers;
using EPR.Calculator.FSS.API.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.FSS.API.Controllers;

/// <summary>
/// Controller for the API to retrieve billings files.
/// </summary>
/// <param name="blobStorageService">A service object that implements <see cref="IBlobStorageService"/>.</param>
/// <param name="runIdValidator">A validator for the run ID.</param>
[Route("api/[controller]")]
public class BillingController(
    IBlobStorageService blobStorageService,
    IValidator<int> runIdValidator)
    : ControllerBase
{
    /// <summary>
    /// API endpoint to retrieve billing details for a given runId.
    /// </summary>
    /// <param name="calculatorRunId">The run ID to retrieve the billings details for.</param>
    /// <returns>The billings details as a string.</returns>
    [HttpGet("billingDetails")]
    public async Task<IActionResult> GetBillingsDetails([FromQuery] int calculatorRunId)
    {
        try
        {
            var validatorResult = runIdValidator.Validate(calculatorRunId);

            if (!validatorResult.IsValid)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = string.Join("; ", validatorResult.Errors.Select(e => e.ErrorMessage)),
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var fileName = BillingFileNameHelper.Create(calculatorRunId);
            var billingData = await blobStorageService.GetFileContents(fileName);

            return billingData;
        }
        catch (FileNotFoundException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "The requested resource could not be found.",
                Detail = "The resource you requested does not exist.",
                Status = StatusCodes.Status404NotFound
            });
        }
    }
}
