using EPR.Calculator.FSS.API.Common;
using EPR.Calculator.FSS.API.Common.Validators;
using FluentValidation;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;

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
        RunIdValidator runIdValidator)
        : Controller
    {
        private IBillingService BillingService { get; init; } = billingService;

        private RunIdValidator RunIdValidator { get; init; } = runIdValidator;

        private TelemetryClient TelemetryClient { get; init; } = telemetryClient;

        /// <summary>
        /// API endpoint to retrieve billing details for a given runId.
        /// </summary>
        /// <param name="runId">The run ID to retrieve the billings details for.</param>
        /// <returns>The billings details as a string.</returns>
        [HttpGet]
        [Route("billingDetails")]
        public async Task<ActionResult<string>> GetBillingsDetails(int runId)
        {
            var validationResult = this.RunIdValidator.Validate(runId);
            if (!validationResult.IsValid)
        {
                this.TelemetryClient.TrackTrace($"RunId \"{runId}\"is invalid.");
                return this.NotFound();
        }

            try
            {
                var billingData = await this.BillingService.GetBillingData(runId);

                this.TelemetryClient.TrackTrace($"Billing data retrieved for runId: {runId} " +
                    $"at {DateTime.Now}, " +
                    $"length {billingData.Length}.");

                return billingData;
            }
            catch (Exception ex) when (ex is KeyNotFoundException || ex is FileNotFoundException)
        {
                this.TelemetryClient.TrackTrace($"Billing data not found for runId \"{runId}\" " +
                    $"at {DateTime.Now}, " +
                    $"error: {ex.Message}.");
                return this.NotFound();
            }
        }
    }
}
