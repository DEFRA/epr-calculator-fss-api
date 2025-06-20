using EPR.Calculator.FSS.API.Common;
using EPR.Calculator.FSS.API.Constants;
using System.Globalization;
using System.Text;

namespace EPR.Calculator.FSS.API
{
    public class BillingService : IBillingService
    {
        private static readonly CompositeFormat BillingFileName
            = CompositeFormat.Parse(BillingConstants.BillFileName);

        private readonly IBlobStorageService storageService;

        public BillingService(
            IBlobStorageService storageService)
        {
            this.storageService = storageService;
        }

        public async Task<string> GetBillingData(int calcRunId)
        {
            // Use the cached CompositeFormat and IFormatProvider for formatting
            string fileName = string.Format(CultureInfo.CurrentCulture, BillingConstants.BillFileName, calcRunId);
            string content = await this.storageService.GetFileContents(fileName);

            return content;
        }
    }
}