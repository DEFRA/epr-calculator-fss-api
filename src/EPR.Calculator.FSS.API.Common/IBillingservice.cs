namespace EPR.Calculator.FSS.API.Common
{
    public interface IBillingService
    {
        /// <summary>
        /// Get the Billing File Data for the calculatorRunId
        /// </summary>
        /// <param name="calcRunId"></param>
        /// <returns></returns>
        Task<string> GetBillingData(int calcRunId);
    }
}
