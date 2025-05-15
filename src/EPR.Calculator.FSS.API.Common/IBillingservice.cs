namespace EPR.Calculator.FSS.API.Common
{
    public interface IBillingService
    {
        /// <summary>
        /// Get the Billing File Data for the calculatorRunId.
        /// </summary>
        /// <param name="calcRunId">The calculation run ID.</param>
        /// <returns>The billings data as a string.</returns>
        Task<string> GetBillingData(int calcRunId);
    }
}