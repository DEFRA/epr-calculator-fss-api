using Azure.Storage;
using Azure.Storage.Blobs;
using EPR.Calculator.FSS.API.Common;
using EPR.Calculator.FSS.API.Constants;
using System.Configuration;
namespace EPR.Calculator.FSS.API
{
    public class StorageService : IStorageService
    {
        public const string BlobStorageSection = "BlobStorage";
        public const string BlobSettingsMissingError = "BlobStorage settings are missing in configuration.";
        public const string ContainerNameMissingError = "Container name is missing in configuration.";
        public const string AccountNameMissingError = "Account name is missing in configuration.";
        public const string OctetStream = "application/octet-stream";
        private readonly BlobContainerClient containerClient;
        private readonly StorageSharedKeyCredential sharedKeyCredential;

        public StorageService(BlobServiceClient blobServiceClient, IConfiguration configuration)
        {
            var settings = configuration.GetSection(BlobStorageSection).Get<BlobStorageSettings>() ?? throw new ConfigurationErrorsException(BlobSettingsMissingError);

            settings.ExtractAccountDetails();

            this.sharedKeyCredential = new StorageSharedKeyCredential(settings.AccountName, settings.AccountKey) ??
                throw new ConfigurationErrorsException(AccountNameMissingError);

            this.containerClient = blobServiceClient.GetBlobContainerClient(settings.ContainerName ??
                throw new ConfigurationErrorsException(ContainerNameMissingError));
        }

        public async Task<string> GetFileContents(string fileName)
        {
            BlobClient blobClient = this.GetBlobClient(fileName);

            if (!await blobClient.ExistsAsync())
            {
                throw new FileNotFoundException(fileName);
            }

            var downloadResult = await blobClient.DownloadContentAsync();
            var content = downloadResult.Value.Content.ToString();
            return content;
        }

        public async Task<bool> IsBlobExistsAsync(string fileName, CancellationToken cancellationToken)
        {
            BlobClient blobClient = this.GetBlobClient(fileName);

            return await blobClient.ExistsAsync(cancellationToken);
        }

        private BlobClient GetBlobClient(string fileName)
        {
            BlobClient? blobClient = this.containerClient.GetBlobClient(fileName);

            return blobClient;
        }
    }
}
