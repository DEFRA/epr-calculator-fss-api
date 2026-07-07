using EPR.Calculator.FSS.API.Configs;
using EPR.Calculator.FSS.API.Helpers;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;

namespace EPR.Calculator.FSS.API.Controllers
{
    /// <summary>
    /// Controller for the API to retrieve billings files.
    /// </summary>
    /// <param name="blobStorageService">A service object that implements <see cref="IBlobStorageService"/>.</param>
    /// <param name="runIdValidator">A validator for the run ID.</param>
    [Route("test-only/Billing")]
    public class TestOnlyController(
        IBlobStorageService blobStorageService,
        IValidator<int> runIdValidator)
        : Controller
    {
        [HttpPost]
        [Route("billingDetails")]
        [Consumes(MediaTypeNames.Application.Json)]
        [SuppressMessage("Security", "S5693", Justification = "Required to support large billing JSON uploads during testing.")]
        [RequestSizeLimit(1_500_000_000)]
        public async Task<IActionResult> UploadBillingDetails(
            [FromQuery] int calculatorRunId,
            [FromServices] IOptions<FeatureManagementSettings> featureManagementSettings)
        {
            if (!featureManagementSettings.Value.EnableBillingUploadEndpoint)
            {
                return NotFound();
            }

            var validationResult = runIdValidator.Validate(calculatorRunId);

            if (!validationResult.IsValid)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                    Status = StatusCodes.Status400BadRequest
                });
            }

            if (!string.Equals(
                    Request.ContentType,
                    MediaTypeNames.Application.Json,
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = "Content-Type must be application/json",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            await blobStorageService.UploadFile(
                fileName: BillingFileNameHelper.Create(calculatorRunId),
                content: Request.Body,
                contentType: MediaTypeNames.Application.Json);

            return Ok();
        }
    }
}