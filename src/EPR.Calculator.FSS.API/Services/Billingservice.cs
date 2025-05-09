using EPR.Calculator.API.Data;
using EPR.Calculator.FSS.API.Common;
using Microsoft.EntityFrameworkCore;
namespace EPR.Calculator.FSS.API
{
    public class BillingService : IBillingService
    {
        private readonly IStorageService storageService;
        private readonly ApplicationDBContext context;

        public BillingService(
            IStorageService storageService,
            ApplicationDBContext context)
        {
            this.storageService = storageService;
            this.context = context;
        }

        public async Task<string> GetBillingData(int calcRunId)
        {
            var calculatorBillingFileMetadata = await this.context.
                CalculatorRunBillingFileMetadata.SingleOrDefaultAsync(x => x.CalculatorRunId == calcRunId);

            if (calculatorBillingFileMetadata != null
                &&
                !string.IsNullOrWhiteSpace(calculatorBillingFileMetadata.BillingJsonFileName))
            {
                var content = await this.storageService.GetFileContents(calculatorBillingFileMetadata.BillingJsonFileName);
                return content;
            }

            throw new NotImplementedException();
        }
    }
}
