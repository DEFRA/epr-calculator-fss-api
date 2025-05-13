using EPR.Calculator.API.Data;
using EPR.Calculator.FSS.API.Common;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.FSS.API
{
    public class BillingService : IBillingService
    {
        private readonly IBlobStorageService storageService;
        private readonly ApplicationDBContext context;

        public BillingService(
            IBlobStorageService storageService,
            ApplicationDBContext context)
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
                throw new KeyNotFoundException($"CalculatorBillingFileMetadata is not available for the calculator Id {calcRunId}");
            }

            var fileName = calculatorBillingFileMetadata.BillingJsonFileName;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                var errorMessage = $"{nameof(calculatorBillingFileMetadata.BillingJsonFileName)} is not available for the calculator Id {calcRunId}";
                throw new KeyNotFoundException(errorMessage);
            }

            var content = await this.storageService.GetFileContents(fileName);

            return content;
        }
    }
}