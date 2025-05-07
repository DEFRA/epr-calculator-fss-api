namespace EPR.Calculator.FSS.API.Common
{
    public class StorageService : IStorageService
    {
        public Task<string> DownloadFile(string fileName, string blobUri)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsBlobExistsAsync(string fileName, string blobUri, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
