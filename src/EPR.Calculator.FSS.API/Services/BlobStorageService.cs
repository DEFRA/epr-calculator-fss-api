using Azure.Storage.Blobs;
using EPR.Calculator.FSS.API.Configs;
using EPR.Calculator.FSS.API.Constants;
using Microsoft.AspNetCore.Mvc;
using System.Configuration;

namespace EPR.Calculator.FSS.API
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly FeatureSettings featureSettings;
        private readonly BlobContainerClient containerClient;
        private readonly BlobContainerClient testContainerClient;

        public BlobStorageService(BlobServiceClient blobServiceClient, IConfiguration configuration)
        {
            this.featureSettings = configuration.GetSection(FeatureSettings.SectionName).Get<FeatureSettings>() ??
                throw new ConfigurationErrorsException("Feature settings are missing in configuration.");

            var settings = configuration.GetSection(BlobStorageSettings.SectionName).Get<BlobStorageSettings>() ??
                throw new ConfigurationErrorsException("BlobStorage settings are missing in configuration.");

            this.containerClient = blobServiceClient.GetBlobContainerClient(settings.ContainerName ??
                throw new ConfigurationErrorsException("Container name is missing in configuration."));

            this.testContainerClient = blobServiceClient.GetBlobContainerClient(settings.TestOnlyContainerName ??
                throw new ConfigurationErrorsException("Test-Only container name is missing in configuration."));

            EnsureContainersExist();
        }

        public async Task<FileStreamResult> GetFileContents(string fileName)
        {
            if (this.featureSettings.EnableBillingUploadEndpoint)
            {
                var testBlobClient = testContainerClient.GetBlobClient(fileName);

                if (await testBlobClient.ExistsAsync())
                {
                    var testDownload = await testBlobClient.OpenReadAsync(new Azure.Storage.Blobs.Models.BlobOpenReadOptions(false));
                    var testProperties = await testBlobClient.GetPropertiesAsync();
                    return new FileStreamResult(testDownload, testProperties.Value.ContentType);
                }
            }

            var blobClient = containerClient.GetBlobClient(fileName);

            if (!await blobClient.ExistsAsync())
            {
                throw new FileNotFoundException(fileName);
            }

            var downloadResult = await blobClient.OpenReadAsync(new Azure.Storage.Blobs.Models.BlobOpenReadOptions(false));
            var properties = await blobClient.GetPropertiesAsync();
            return new FileStreamResult(downloadResult, properties.Value.ContentType);
        }

        public async Task UploadFile(string fileName, Stream content, string contentType)
        {
            await testContainerClient.GetBlobClient(fileName).UploadAsync(
                content: content,
                options: new Azure.Storage.Blobs.Models.BlobUploadOptions
                {
                    HttpHeaders = new Azure.Storage.Blobs.Models.BlobHttpHeaders
                    {
                        ContentType = contentType,
                    }
                });
        }

        private async Task EnsureContainersExist()
        {
            await containerClient.CreateIfNotExistsAsync();
            await testContainerClient.CreateIfNotExistsAsync();
        }
    }
}