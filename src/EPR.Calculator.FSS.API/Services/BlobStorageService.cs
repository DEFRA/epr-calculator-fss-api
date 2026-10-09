using Azure.Storage.Blobs;
using EPR.Calculator.FSS.API.Configs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.FSS.API.Services;

public interface IBlobStorageService
{
    /// <summary>
    /// Downloads a file from the test-only blob storage container.
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
    private readonly BlobContainerClient testContainerClient;

    public BlobStorageService(BlobServiceClient blobServiceClient, IOptions<BlobStorageSettings> blobStorageSettings)
    {
        this.testContainerClient = blobServiceClient.GetBlobContainerClient(blobStorageSettings.Value.TestOnlyContainerName);

        _ = EnsureContainerExists();
    }

    public async Task<FileStreamResult> GetFileContents(string fileName)
    {
        var blobClient = testContainerClient.GetBlobClient(fileName);

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

    private async Task EnsureContainerExists()
    {
        await testContainerClient.CreateIfNotExistsAsync();
    }
}
