using EPR.Calculator.FSS.API.Common;
using EPR.Calculator.FSS.API.Constants;
using EPR.Calculator.FSS.API.Helpers;
using FluentValidation;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Mime;

namespace EPR.Calculator.FSS.API.Controllers
{
    /// <summary>
    /// Controller for the API to retrieve billings files.
    /// </summary>
    /// <param name="billingService">A service object that implements <see cref="IBillingService"/>.</param>
    /// <param name="telemetryClient">A <see cref="TelemetryClient"/>.</param>
    /// <param name="runIdValidator">A validator for the run ID.</param>
    [Route("api/test-only")]
    public class TestOnlyController(
        IBlobStorageService blobStorageService,
        IValidator<int> runIdValidator)
        : Controller
    {
        [HttpPost]
        [Route("billingDetails")]
        [Consumes(MediaTypeNames.Application.Json)]
        [RequestSizeLimit(150_000_000)] // ~150 MB
        public async Task<IActionResult> UploadBillingDetails(
            [FromQuery] int calculatorRunId,
            [FromServices] IOptions<FeatureSettings> features)
        {
            if (!features.Value.EnableBillingUploadEndpoint)
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
                Request.Body,
                MediaTypeNames.Application.Json);

            return Ok();
        }
    }
}