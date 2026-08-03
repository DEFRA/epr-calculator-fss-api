using System.Configuration;
using Azure.Storage.Blobs;
using EPR.Calculator.FSS.API.Configs;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.FSS.API.Services;


public interface IBlobStorageService
{
    /// <summary>
    /// Downloads a file from the specified blob storage.
    /// </summary>
    /// <param name="fileName">The name of the file to download.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the download result.</returns>
    public Task<FileStreamResult> GetFileContents(string fileName);

    /// <summary>
    /// Stream a file into storage - test only behaviour.
    /// </summary>
    /// <param name="fileName">The file name to store.</param>
    /// <param name="content">The stream of content to store.</param>
    /// <param name="contentType">The content type for the stream.</param>
    /// <returns>The billings data as a string.</returns>
    public Task UploadFile(string fileName, Stream content, string contentType);
}

public class BlobStorageService : IBlobStorageService
{
    private readonly FeatureManagementSettings featureManagementSettings;
    private readonly BlobContainerClient containerClient;
    private readonly BlobContainerClient testContainerClient;

    public BlobStorageService(BlobServiceClient blobServiceClient, IConfiguration configuration)
    {
        this.featureManagementSettings = configuration.GetSection(FeatureManagementSettings.SectionName).Get<FeatureManagementSettings>() ??
            throw new ConfigurationErrorsException("FeatureManagement settings are missing in configuration.");

        var settings = configuration.GetSection(BlobStorageSettings.SectionName).Get<BlobStorageSettings>() ??
            throw new ConfigurationErrorsException("BlobStorage settings are missing in configuration.");

        this.containerClient = blobServiceClient.GetBlobContainerClient(settings.ContainerName ??
            throw new ConfigurationErrorsException("Container name is missing in configuration."));

        this.testContainerClient = blobServiceClient.GetBlobContainerClient(settings.TestOnlyContainerName ??
            throw new ConfigurationErrorsException("Test-Only container name is missing in configuration."));

        _ = EnsureContainersExist();
    }

    public async Task<FileStreamResult> GetFileContents(string fileName)
    {
        if (this.featureManagementSettings.EnableBillingUploadEndpoint)
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
