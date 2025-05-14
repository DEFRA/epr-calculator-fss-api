using EPR.Calculator.FSS.API.Common;
using EPR.Calculator.FSS.API.Common.Properties;
using EPR.Calculator.FSS.API.Common.Validators;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
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
        RunIdValidator runIdValidator)
        : Controller
    {
        private static readonly CompositeFormat RunIdIsInvalid
            = CompositeFormat.Parse(Resources.RunIdIsInvalid);

        private static readonly CompositeFormat BillingDataRetrieved
            = CompositeFormat.Parse(Resources.BillingDataRetrieved);

        private static readonly CompositeFormat BillingDataNotFound
            = CompositeFormat.Parse(Resources.BillingDataNotFound);

        private static readonly CompositeFormat BillingDataMiscError
            = CompositeFormat.Parse(Resources.BillingDataMiscError);

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
            if (!this.ModelState.IsValid)
            {
                this.TelemetryClient.TrackTrace(string.Format(CultureInfo.CurrentCulture, RunIdIsInvalid, runId));
                return this.NotFound();
            }

            try
            {
                var billingData = await this.BillingService.GetBillingData(runId);

                this.TelemetryClient.TrackTrace(string.Format(
                    CultureInfo.CurrentCulture,
                    BillingDataRetrieved,
                    runId,
                    DateTime.UtcNow,
                    billingData.Length));

                return billingData;
            }
            catch (Exception ex) when (ex is KeyNotFoundException || ex is FileNotFoundException)
            {
                this.TelemetryClient.TrackTrace(string.Format(
                    CultureInfo.CurrentCulture,
                    BillingDataNotFound,
                    runId,
                    DateTime.UtcNow,
                    ex.Message));

                return this.NotFound();
            }
            catch(Exception ex)
            {
                this.TelemetryClient.TrackTrace(string.Format(
                    CultureInfo.CurrentCulture,
                    BillingDataMiscError,
                    runId,
                    DateTime.UtcNow,
                    ex.Message));

                return this.StatusCode(500);
            }
        }
    }
}