namespace EPR.Calculator.FSS.API.Common
{
    public class Billingservice : IBillingservice
    {
        private readonly IStorageService storageService;
        public Billingservice(IStorageService storageService)
        {
            this.storageService = storageService;
        }

        public Task<string> GetBillingData(int calcRunId)
        {
            throw new NotImplementedException();
        }
    }
}
