using EPR.Calculator.FSS.API.Common;
using EPR.Calculator.FSS.API.Common.Properties;
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
    /// <param name="billingService">A service object that implements <see cref="IBillingService"/>.</param>
    /// <param name="telemetryClient">A <see cref="TelemetryClient"/>.</param>
    /// <param name="runIdValidator">A validator for the run ID.</param>
    [Route("api/[controller]")]
    public class BillingController(
        IBillingService billingService,
        TelemetryClient telemetryClient,
        IValidator<int> runIdValidator)
        : Controller
    {
        private static readonly CompositeFormat RunIdIsInvalid
            = CompositeFormat.Parse(Resources.RunIdIsInvalid);

        private static readonly CompositeFormat BillingDataRetrieved
            = CompositeFormat.Parse(Resources.BillingDataRetrieved);

        private IBillingService BillingService { get; init; } = billingService;

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

                var billingData = await this.BillingService.GetBillingData(calculatorRunId);

                this.TelemetryClient.TrackTrace(string.Format(
                    CultureInfo.CurrentCulture,
                    BillingDataRetrieved,
                    calculatorRunId,
                    DateTime.UtcNow,
                    billingData.Length));

                return Content(billingData, MediaTypeNames.Application.Json);
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