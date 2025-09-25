using EPR.Calculator.FSS.API.Common;
using EPR.Calculator.FSS.API.Constants;
using System.Globalization;
using System.Text;

namespace EPR.Calculator.FSS.API
{
    public class BillingService(IBlobStorageService storageService) : IBillingService
    {
        private static readonly CompositeFormat BillingFileName
            = CompositeFormat.Parse(BillingConstants.BillFileName);

        public async Task<string> GetBillingData(int calcRunId)
        {
            // Use the cached CompositeFormat and IFormatProvider for formatting
            string fileName = string.Format(CultureInfo.CurrentCulture, BillingFileName, calcRunId);
            string content = await storageService.GetFileContents(fileName);

            return content;
        }
    }
}