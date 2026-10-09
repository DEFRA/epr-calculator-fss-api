using EPR.Calculator.FSS.API.Configs;
using EPR.Calculator.FSS.API.Helpers;
using EPR.Calculator.FSS.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;

namespace EPR.Calculator.FSS.API.UnitTests.Services;

[TestClass]
public class BillingServiceTests
{
    private readonly Mock<IBlobStorageService> mockBlobStorageService = new();
    private readonly Mock<IDownloadService> mockDownloadService = new();

    private BillingService CreateService(bool enableBillingUploadEndpoint) =>
        new(
            mockBlobStorageService.Object,
            mockDownloadService.Object,
            Options.Create(new FeatureManagementSettings
            {
                EnableBillingUploadEndpoint = enableBillingUploadEndpoint,
            }));

    [TestMethod]
    public async Task GetBillingFile_WhenUploadEndpointEnabled_ReturnsFromBlobStorage()
    {
        // Arrange
        const int runId = 42;
        var expectedFileName = BillingFileNameHelper.Create(runId);
        var service = CreateService(enableBillingUploadEndpoint: true);

        var billingFile = new FileStreamResult(new MemoryStream(), "application/json");

        mockBlobStorageService.Setup(s => s.GetFileContents(expectedFileName))
            .ReturnsAsync(billingFile);

        // Act
        var result = await service.GetBillingFile(runId, CancellationToken.None);

        // Assert
        Assert.AreSame(billingFile, result);

        mockBlobStorageService.Verify(s => s.GetFileContents(expectedFileName), Times.Once);
        mockDownloadService.Verify(
            s => s.DownloadFile(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task GetBillingFile_WhenUploadEndpointEnabledButFileNotFound_FallsBackToDownloadService()
    {
        // Arrange
        const int runId = 42;
        var expectedFileName = BillingFileNameHelper.Create(runId);
        var service = CreateService(enableBillingUploadEndpoint: true);

        var billingFile = new FileStreamResult(new MemoryStream(), "application/json");

        mockBlobStorageService.Setup(s => s.GetFileContents(expectedFileName))
            .ThrowsAsync(new FileNotFoundException());

        mockDownloadService.Setup(s => s.DownloadFile(runId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(billingFile);

        // Act
        var result = await service.GetBillingFile(runId, CancellationToken.None);

        // Assert
        Assert.AreSame(billingFile, result);

        mockBlobStorageService.Verify(s => s.GetFileContents(expectedFileName), Times.Once);
        mockDownloadService.Verify(s => s.DownloadFile(runId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task GetBillingFile_WhenUploadEndpointDisabled_ReturnsFromDownloadService()
    {
        // Arrange
        const int runId = 42;
        var service = CreateService(enableBillingUploadEndpoint: false);

        var billingFile = new FileStreamResult(new MemoryStream(), "application/json");

        mockDownloadService.Setup(s => s.DownloadFile(runId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(billingFile);

        // Act
        var result = await service.GetBillingFile(runId, CancellationToken.None);

        // Assert
        Assert.AreSame(billingFile, result);

        mockDownloadService.Verify(s => s.DownloadFile(runId, It.IsAny<CancellationToken>()), Times.Once);
        mockBlobStorageService.Verify(s => s.GetFileContents(It.IsAny<string>()), Times.Never);
    }
}
