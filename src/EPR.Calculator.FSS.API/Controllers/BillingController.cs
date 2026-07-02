using EPR.Calculator.FSS.API.Common;
using EPR.Calculator.FSS.API.Common.Properties;
using EPR.Calculator.FSS.API.Helpers;
using FluentValidation;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net.Mime;
using System.Text;

namespace EPR.Calculator.FSS.API.Controllers
{
    /// <summary>
    /// Controller for the API to retrieve billings files.
    /// </summary>
    /// <param name="blobStorageService">A service object that implements <see cref="IBlobStorageService"/>.</param>
    /// <param name="telemetryClient">A <see cref="TelemetryClient"/>.</param>
    /// <param name="runIdValidator">A validator for the run ID.</param>
    [Route("api/[controller]")]
    public class BillingController(
        IBlobStorageService blobStorageService,
        TelemetryClient telemetryClient,
        IValidator<int> runIdValidator)
        : Controller
    {
        private static readonly CompositeFormat RunIdIsInvalid
            = CompositeFormat.Parse(Resources.RunIdIsInvalid);

        private static readonly CompositeFormat BillingDataRetrieved
            = CompositeFormat.Parse(Resources.BillingDataRetrieved);

        private IBlobStorageService BlobStorageService { get; init; } = blobStorageService;

        private IValidator<int> RunIdValidator { get; init; } = runIdValidator;

        private TelemetryClient TelemetryClient { get; init; } = telemetryClient;

        /// <summary>
        /// API endpoint to retrieve billing details for a given runId.
        /// </summary>
        /// <param name="calculatorRunId">The run ID to retrieve the billings details for.</param>
        /// <returns>The billings details as a string.</returns>
        [HttpGet]
        [Route("billingDetails")]
        public async Task<IActionResult> GetBillingsDetails([FromQuery] int calculatorRunId)
        {
            try
            {
                var validatorResult = RunIdValidator.Validate(calculatorRunId);

                if (!validatorResult.IsValid)
                {
                    this.TelemetryClient.TrackTrace(string.Format(CultureInfo.CurrentCulture, RunIdIsInvalid, calculatorRunId));

                    return BadRequest(new ProblemDetails
                    {
                        Title = "Validation Error",
                        Detail = string.Join("; ", validatorResult.Errors.Select(e => e.ErrorMessage)),
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                var fileName = BillingFileNameHelper.Create(calculatorRunId);
                var billingData = await this.BlobStorageService.GetFileContents(fileName);

                this.TelemetryClient.TrackTrace(string.Format(
                    CultureInfo.CurrentCulture,
                    BillingDataRetrieved,
                    calculatorRunId,
                    DateTime.UtcNow,
                    billingData.FileStream.Length));

                return billingData;
            }
            catch (Exception ex) when (ex is FileNotFoundException)
            {
                this.TelemetryClient.TrackException(ex);

                return NotFound(new ProblemDetails
                {
                    Title = "The requested resource could not be found.",
                    Detail = "The resource you requested does not exist.",
                    Status = StatusCodes.Status404NotFound
                });
            }
            catch(Exception ex)
            {
                this.TelemetryClient.TrackException(ex);
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }
    }
}