using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.FSS.API;

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
