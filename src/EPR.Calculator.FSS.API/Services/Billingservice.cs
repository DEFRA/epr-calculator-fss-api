using EPR.Calculator.FSS.API.Common;
using EPR.Calculator.FSS.API.Common.Properties;
using EPR.Calculator.FSS.API.Constants;
using System.Text;

namespace EPR.Calculator.FSS.API
{
    public class BillingService : IBillingService
    {
        private static readonly CompositeFormat BillingDataNotFound
            = CompositeFormat.Parse(Resources.BillingDataNotFound);

        private static readonly CompositeFormat BillingDataUnavaliable
            = CompositeFormat.Parse(Resources.BillingDataUnavaliable);

        private readonly IBlobStorageService storageService;

        public BillingService(
            IBlobStorageService storageService)
        {
            this.storageService = storageService;
        }

        public async Task<string> GetBillingData(int calcRunId)
        {
            var fileName = $"{calcRunId}{BillingConstants.BillFileNameEndSuffix}.json";
            var content = await this.storageService.GetFileContents(fileName);

            return content;
        }
    }
}