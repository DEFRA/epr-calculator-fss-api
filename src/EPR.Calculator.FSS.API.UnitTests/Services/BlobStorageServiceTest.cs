using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Moq;

namespace EPR.Calculator.FSS.API.UnitTests.Services
{
    [TestClass]
    public class BlobStorageServiceTest
    {
        private Mock<BlobServiceClient> mockBlobServiceClient;
        private BlobStorageService blobStorageService;
        private Mock<BlobContainerClient> mockBlobContainerClient;
        private Mock<BlobClient> mockBlobClient;

        [TestInitialize]
        public void Init()
        {
            this.mockBlobContainerClient = new Mock<BlobContainerClient>();
            this.mockBlobServiceClient = new Mock<BlobServiceClient>();
            this.mockBlobClient = new Mock<BlobClient>();
            var configs = ConfigurationItems.GetConfigurationValues();

            this.mockBlobContainerClient.Setup(x => x.GetBlobClient(It.IsAny<string>()))
                .Returns(this.mockBlobClient.Object);

            this.mockBlobServiceClient.Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(this.mockBlobContainerClient.Object);

            this.blobStorageService = new BlobStorageService(
                this.mockBlobServiceClient.Object,
                configs);

            this.mockBlobClient.Setup(x => x.ExistsAsync(default))
                .ReturnsAsync(Response.FromValue(true, null!));

            var content = "test content";
            var binaryData = BinaryData.FromString(content);
            var downloadDetails = BlobsModelFactory.BlobDownloadDetails(
                contentLength: content.Length,
                contentType: "application/octet-stream");

            var downloadResult = BlobsModelFactory.BlobDownloadResult(
                content: binaryData,
                details: downloadDetails);
            this.mockBlobClient.Setup(x => x.DownloadContentAsync()).ReturnsAsync(Response.FromValue(downloadResult, null!));
        }

        [TestMethod]
        public async Task GetFileContents_WhenFileExists_ReturnsContents()
        {
            using CancellationTokenSource cancellationTokenSource = new();
            var fileName = "test.txt";
            var blobUri = "https://example.com/test.txt";

            this.mockBlobClient.Setup(x => x.ExistsAsync(cancellationTokenSource.Token)).ReturnsAsync(Response.FromValue(true, null!));
            this.mockBlobClient.Setup(x => x.Uri).Returns(new Uri(blobUri));
            blobUri = string.Empty;

            var result = await this.blobStorageService.GetFileContents(fileName);

            Assert.IsNotNull(result);
            Assert.AreEqual("test content", result);
        }
    }
}