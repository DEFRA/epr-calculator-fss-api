using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EPR.Calculator.FSS.API.Configs;
using EPR.Calculator.FSS.API.Services;
using Microsoft.Extensions.Options;
using Moq;
using System.Text;

namespace EPR.Calculator.FSS.API.UnitTests.Services
{
    [TestClass]
    public class BlobStorageServiceTest
    {
        private const string TestOnlyContainerName = "TestOnlyContainerName";

        private BlobStorageService blobStorageService = null!;
        private Mock<BlobServiceClient> mockBlobServiceClient = null!;
        private Mock<BlobContainerClient> mockTestBlobContainerClient = null!;
        private Mock<BlobClient> mockTestBlobClient = null!;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Init()
        {
            mockBlobServiceClient = new Mock<BlobServiceClient>();
            mockTestBlobContainerClient = new Mock<BlobContainerClient>();
            mockTestBlobClient = new Mock<BlobClient>();

            mockTestBlobClient
                .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(true, null!));

            mockTestBlobClient
                .Setup(x => x.OpenReadAsync(
                    It.IsAny<BlobOpenReadOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Stream>(
                    new MemoryStream(Encoding.UTF8.GetBytes("test content"))));

            mockTestBlobClient
                .Setup(x => x.GetPropertiesAsync(
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(
                    BlobsModelFactory.BlobProperties(
                        contentType: "application/octet-stream"),
                    null!));

            mockTestBlobContainerClient
                .Setup(x => x.GetBlobClient(It.IsAny<string>()))
                .Returns(mockTestBlobClient.Object);

            mockBlobServiceClient
                .Setup(x => x.GetBlobContainerClient(TestOnlyContainerName))
                .Returns(mockTestBlobContainerClient.Object);

            blobStorageService = new BlobStorageService(
                mockBlobServiceClient.Object,
                Options.Create(new BlobStorageSettings
                {
                    TestOnlyContainerName = TestOnlyContainerName,
                }));
        }

        [TestMethod]
        public async Task GetFileContents_WhenFileDoesNotExist_ThrowsFileNotFoundException()
        {
            // Arrange
            var fileName = "missing.txt";

            mockTestBlobClient
                .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(false, null!));

            // Act / Assert
            await Assert.ThrowsExactlyAsync<FileNotFoundException>(
                () => blobStorageService.GetFileContents(fileName));
        }

        [TestMethod]
        public async Task GetFileContents_WhenFileExists_ReturnsContents()
        {
            var result = await this.blobStorageService.GetFileContents("test.txt");
            using var reader = new StreamReader(result.FileStream);
            var content = await reader.ReadToEndAsync(TestContext.CancellationTokenSource.Token);

            Assert.IsNotNull(result);
            Assert.AreEqual("test content", content);

            this.mockTestBlobClient.Verify(
                x => x.OpenReadAsync(
                    It.IsAny<BlobOpenReadOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
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
