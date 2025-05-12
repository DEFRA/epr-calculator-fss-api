namespace EPR.Calculator.FSS.API.Common
{
    public interface IBlobStorageService
    {
        /// <summary>
        /// Downloads a file from the specified blob storage.
        /// </summary>
        /// <param name="fileName">The name of the file to download.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the download result.</returns>
        Task<string> GetFileContents(string fileName);

        /// <summary>
        /// Checks if a blob exists in the specified blob storage.
        /// </summary>
        /// <param name="fileName">The name of the blob to check.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the blob exists.</returns>
        Task<bool> IsBlobExistsAsync(string fileName);
    }
}
