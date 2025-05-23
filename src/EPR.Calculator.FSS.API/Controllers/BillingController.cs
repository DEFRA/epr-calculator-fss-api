﻿using EPR.Calculator.FSS.API.Common;
using EPR.Calculator.FSS.API.Common.Properties;
using EPR.Calculator.FSS.API.Common.Validators;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;
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
        /// <param name="calculatorRunId">The run ID to retrieve the billings details for.</param>
        /// <returns>The billings details as a string.</returns>
        [HttpGet]
        [Route("billingDetails")]
        public async Task<IResult> GetBillingsDetails(int calculatorRunId)
        {
            if (!this.ModelState.IsValid)
            {
                this.TelemetryClient.TrackTrace(string.Format(CultureInfo.CurrentCulture, RunIdIsInvalid, calculatorRunId));
                return Results.BadRequest();
            }

            try
            {
                var billingData = await this.BillingService.GetBillingData(calculatorRunId);

                this.TelemetryClient.TrackTrace(string.Format(
                    CultureInfo.CurrentCulture,
                    BillingDataRetrieved,
                    calculatorRunId,
                    DateTime.UtcNow,
                    billingData.Length));

                return Results.Content(billingData, MediaTypeNames.Application.Json);
            }
            catch (Exception ex) when (ex is KeyNotFoundException || ex is FileNotFoundException)
            {
                this.TelemetryClient.TrackException(ex);
                return Results.NotFound();
            }
            catch(Exception ex)
            {
                this.TelemetryClient.TrackException(ex);
                return Results.StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}