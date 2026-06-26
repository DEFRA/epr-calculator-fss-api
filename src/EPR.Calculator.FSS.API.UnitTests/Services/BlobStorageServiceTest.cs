using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Moq;
using System.Text;

namespace EPR.Calculator.FSS.API.UnitTests.Services
{
    [TestClass]
    public class BlobStorageServiceTest
    {
        private BlobStorageService blobStorageService = null!;
        private Mock<BlobServiceClient> mockBlobServiceClient = null!;
        private Mock<BlobContainerClient> mockBlobContainerClient = null!;
        private Mock<BlobContainerClient> mockTestBlobContainerClient = null!;
        private Mock<BlobClient> mockBlobClient = null!;
        private Mock<BlobClient> mockTestBlobClient = null!;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Init()
        {
            static void SetupBlobClient(Mock<BlobClient> blobClient, string content)
            {
                blobClient
                    .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Response.FromValue(true, null!));

                blobClient
                    .Setup(x => x.OpenReadAsync(
                        It.IsAny<BlobOpenReadOptions>(),
                        It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult<Stream>(
                        new MemoryStream(Encoding.UTF8.GetBytes(content))));

                blobClient
                    .Setup(x => x.GetPropertiesAsync(
                        null,
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Response.FromValue(
                        BlobsModelFactory.BlobProperties(
                            contentType: "application/octet-stream"),
                        null!));
            }

            mockBlobServiceClient = new Mock<BlobServiceClient>();

            mockBlobContainerClient = new Mock<BlobContainerClient>();
            mockTestBlobContainerClient = new Mock<BlobContainerClient>();

            mockBlobClient = new Mock<BlobClient>();
            mockTestBlobClient = new Mock<BlobClient>();

            var config = ConfigurationItems.GetConfigurationValues();

            mockBlobContainerClient
                .Setup(x => x.GetBlobClient(It.IsAny<string>()))
                .Returns(mockBlobClient.Object);

            mockTestBlobContainerClient
                .Setup(x => x.GetBlobClient(It.IsAny<string>()))
                .Returns(mockTestBlobClient.Object);

            mockBlobServiceClient
                .Setup(x => x.GetBlobContainerClient(config["BlobStorage:ContainerName"]))
                .Returns(mockBlobContainerClient.Object);

            mockBlobServiceClient
                .Setup(x => x.GetBlobContainerClient(config["BlobStorage:TestOnlyContainerName"]))
                .Returns(mockTestBlobContainerClient.Object);

            SetupBlobClient(mockBlobClient, "main content");
            SetupBlobClient(mockTestBlobClient, "test content");

            blobStorageService = new BlobStorageService(
                mockBlobServiceClient.Object,
                config);
        }

        [TestMethod]
        public async Task GetFileContents_WhenFileDoesNotExist_ThrowsFileNotFoundException()
        {
            // Arrange
            var fileName = "missing.txt";

            mockBlobClient
                .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(false, null!));

            // Act / Assert
            await Assert.ThrowsExactlyAsync<FileNotFoundException>(
                () => blobStorageService.GetFileContents(fileName));
        }

        [TestMethod]
        public async Task GetFileContents_WhenFileExists_ReturnsContents()
        {
            using CancellationTokenSource cancellationTokenSource = new();
            this.mockBlobClient
                .Setup(x => x.ExistsAsync(cancellationTokenSource.Token))
                .ReturnsAsync(Response.FromValue(true, null!));

            var result = await this.blobStorageService.GetFileContents("test.txt");
            using var reader = new StreamReader(result.FileStream);
            var content = await reader.ReadToEndAsync(TestContext.CancellationTokenSource.Token);

            Assert.IsNotNull(result);
            Assert.AreEqual("main content", content);

            this.mockTestBlobClient.Verify(
                x => x.OpenReadAsync(
                    It.IsAny<BlobOpenReadOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            this.mockBlobClient.Verify(
                x => x.OpenReadAsync(
                    It.IsAny<BlobOpenReadOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [TestMethod]
        public async Task GetFileContents_WhenBillingUploadEndpointEnabled_ReturnsTestContainerFile()
        {
            var config = ConfigurationItems.GetConfigurationValues();
            config["FeatureManagement:EnableBillingUploadEndpoint"] = "true";
            var service = new BlobStorageService(this.mockBlobServiceClient.Object, config);

            using CancellationTokenSource cancellationTokenSource = new();
            this.mockTestBlobClient
                .Setup(x => x.ExistsAsync(cancellationTokenSource.Token))
                .ReturnsAsync(Response.FromValue(true, null!));

            var result = await service.GetFileContents("test.txt");
            using var reader = new StreamReader(result.FileStream);
            var content = await reader.ReadToEndAsync(TestContext.CancellationTokenSource.Token);

            Assert.IsNotNull(result);
            Assert.AreEqual("test content", content);

            this.mockTestBlobClient.Verify(
                x => x.OpenReadAsync(
                    It.IsAny<BlobOpenReadOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.mockBlobClient.Verify(
                x => x.OpenReadAsync(
                    It.IsAny<BlobOpenReadOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [TestMethod]
        public async Task UploadFile_UploadsFile_WithCorrectContentType()
        {
            await using var stream = new MemoryStream("""{"field1":"value1"}"""u8.ToArray());

            this.mockTestBlobClient
                .Setup(x => x.UploadAsync(
                    It.IsAny<Stream>(),
                    It.IsAny<BlobUploadOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<Response<BlobContentInfo>>());

            await this.blobStorageService.UploadFile(
                "test.json",
                stream,
                "application/json");

            this.mockTestBlobClient.Verify(
                x => x.UploadAsync(
                    It.IsAny<Stream>(),
                    It.Is<BlobUploadOptions>(o =>
                        o.HttpHeaders!.ContentType == "application/json"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
