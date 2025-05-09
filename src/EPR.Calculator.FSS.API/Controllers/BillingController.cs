using EPR.Calculator.FSS.API.Common;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.FSS.API.Controllers
{
    [Route("api/[controller]")]
    public class BillingController
    {
        private readonly IBillingservice billingService;

        public BillingController(IBillingservice billingService)
        {
            this.billingService = billingService;
        }

        [HttpGet]
        [Route("billingDetails")]
        public async Task<string> GetBillingsDetails(int runId)
        {
            return await this.billingService.GetBillingData(runId);
        }
    }
}
