using Azure;
using Azure.Storage.Blobs;
using Moq;

namespace EPR.Calculator.FSS.API.UnitTests
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
        }

        [TestMethod]
        public async Task IsBlobExistsAsync_ShouldReturnTrue_WhenFileExists()
        {
            using CancellationTokenSource cancellationTokenSource = new();
            var fileName = "test.txt";
            var blobUri = "https://example.com/test.txt";

            this.mockBlobClient.Setup(x => x.ExistsAsync(cancellationTokenSource.Token)).ReturnsAsync(Response.FromValue(true, null!));
            this.mockBlobClient.Setup(x => x.Uri).Returns(new Uri(blobUri));
            blobUri = string.Empty;

            bool result = await this.blobStorageService.IsBlobExistsAsync(fileName);

            Assert.IsTrue(result);
        }
    }
}
