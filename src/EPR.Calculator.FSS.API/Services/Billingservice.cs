using EPR.Calculator.API.Data;
using EPR.Calculator.FSS.API.Common;
namespace EPR.Calculator.FSS.API
{
    public class Billingservice : IBillingservice
    {
        private readonly IStorageService storageService;
        private readonly ApplicationDBContext context;

        public Billingservice(
            IStorageService storageService,
            ApplicationDBContext context)
        {
            this.storageService = storageService;
            this.context = context;
        }

        public Task<string> GetBillingData(int calcRunId)
        {
            // this.context.

            throw new NotImplementedException();
        }
    }
}
