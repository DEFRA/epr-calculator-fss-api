namespace EPR.Calculator.FSS.API.Common
{
    public interface IBillingservice
    {
        /// <summary>
        /// Get the Billing File Data for the calculatorRunId
        /// </summary>
        /// <param name="calcRunId"></param>
        /// <returns></returns>
        Task<string> GetBillingData(int calcRunId);
    }
}
