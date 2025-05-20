using EPR.Calculator.API.Data;
using EPR.Calculator.FSS.API.Common;
using EPR.Calculator.FSS.API.Common.Properties;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
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
        private readonly ApplicationDBContext context;

        public BillingService(
            IBlobStorageService storageService,
            ApplicationDBContextWrapper context)
        {
            this.storageService = storageService;
            this.context = context;
        }

        public async Task<string> GetBillingData(int calcRunId)
        {
            var calculatorBillingFileMetadata = await this.context.
                CalculatorRunBillingFileMetadata.
                SingleOrDefaultAsync(x => x.CalculatorRunId == calcRunId);
            if (calculatorBillingFileMetadata == null)
            {
                throw new KeyNotFoundException(string.Format(
                    CultureInfo.CurrentCulture,
                    BillingDataNotFound,
                    calcRunId));
            }

            var fileName = calculatorBillingFileMetadata.BillingJsonFileName;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                var errorMessage = string.Format(
                    CultureInfo.CurrentCulture,
                    BillingDataUnavaliable,
                    nameof(calculatorBillingFileMetadata.BillingJsonFileName),
                    calcRunId);
                throw new KeyNotFoundException(errorMessage);
            }

            var content = await this.storageService.GetFileContents(fileName);

            return content;
        }
    }
}