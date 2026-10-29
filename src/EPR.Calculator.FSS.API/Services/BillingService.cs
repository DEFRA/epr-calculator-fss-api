using EPR.Calculator.FSS.API.Configs;
using EPR.Calculator.FSS.API.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.FSS.API.Services;

public interface IBillingService
{
    /// <summary>
    /// Gets the billing file for the given calculator run - from the test blob storage container
    /// when <see cref="FeatureManagementSettings.EnableBillingUploadEndpoint"/> is enabled, falling
    /// back to the live EPR Calculator API if the test file isn't found there, or if the flag is
    /// disabled altogether.
    /// </summary>
    /// <param name="calculatorRunId">The run ID to retrieve the billing file for.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the billing file.</returns>
    Task<FileStreamResult> GetBillingFile(int calculatorRunId, CancellationToken cancellationToken);
}

public class BillingService(
    IBlobStorageService blobStorageService,
    IDownloadService downloadService,
    IOptions<FeatureManagementSettings> featureManagementSettings)
    : IBillingService
{
    public async Task<FileStreamResult> GetBillingFile(int calculatorRunId, CancellationToken cancellationToken)
    {
        if (featureManagementSettings.Value.EnableBillingUploadEndpoint)
        {
            try
            {
                var fileName = BillingFileNameHelper.Create(calculatorRunId);
                return await blobStorageService.GetFileContents(fileName);
            }
            catch (FileNotFoundException)
            {
                // Fall back to the calculator API if the test file isn't found.
            }
        }

        return await downloadService.DownloadFile(calculatorRunId, cancellationToken);
    }
}
