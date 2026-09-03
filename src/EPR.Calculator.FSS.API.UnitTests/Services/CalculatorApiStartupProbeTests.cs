using EPR.Calculator.FSS.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Calculator.FSS.API.UnitTests.Services;

[TestClass]
public class CalculatorApiStartupProbeTests
{
    private readonly Mock<IDownloadService> mockDownloadService = new();
    private readonly Mock<ILogger<CalculatorApiStartupProbe>> mockLogger = new();
    private CalculatorApiStartupProbe probe = null!;

    [TestInitialize]
    public void Setup()
    {
        probe = new CalculatorApiStartupProbe(mockDownloadService.Object, mockLogger.Object, TimeSpan.FromSeconds(1));
    }

    [TestMethod]
    public async Task ProbeAsync_WhenCalculatorApiReportsFileNotFound_LogsSuccessAndDoesNotThrow()
    {
        // Arrange
        mockDownloadService
            .Setup(s => s.DownloadFile(-1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FileNotFoundException());

        // Act
        await probe.ProbeAsync(CancellationToken.None);

        // Assert
        VerifyLog(LogLevel.Information, Times.Once());
        VerifyLog(LogLevel.Error, Times.Never());
    }

    [TestMethod]
    public async Task ExecuteAsync_WaitsForStartupDelayThenRunsProbe()
    {
        // Arrange
        mockDownloadService
            .Setup(s => s.DownloadFile(-1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FileNotFoundException());

        // Act
        await probe.StartAsync(CancellationToken.None);
        await probe.ExecuteTask!;

        // Assert
        VerifyLog(LogLevel.Information, Times.Once());
    }

    [TestMethod]
    public async Task ProbeAsync_WhenCallFails_LogsInfoAndDoesNotThrow()
    {
        // Arrange
        mockDownloadService
            .Setup(s => s.DownloadFile(-1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("unauthorized"));

        // Act
        await probe.ProbeAsync(CancellationToken.None);

        // Assert
        VerifyLog(LogLevel.Information, Times.Once());
    }

    [TestMethod]
    public async Task ProbeAsync_WhenCalculatorApiUnexpectedlyReturnsContent_DisposesStreamAndLogsInformation()
    {
        // Arrange
        var streamContent = new MemoryStream("{}"u8.ToArray());
        var result = new FileStreamResult(streamContent, "application/json");

        mockDownloadService
            .Setup(s => s.DownloadFile(-1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        await probe.ProbeAsync(CancellationToken.None);

        // Assert
        VerifyLog(LogLevel.Information, Times.Once());
        Assert.IsFalse(streamContent.CanRead, "the response stream should have been disposed");
    }

    [TestMethod]
    public async Task ProbeAsync_PassesProbeRunIdAndCancellationTokenThrough()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();

        mockDownloadService
            .Setup(s => s.DownloadFile(-1, cancellationTokenSource.Token))
            .ThrowsAsync(new FileNotFoundException());

        // Act
        await probe.ProbeAsync(cancellationTokenSource.Token);

        // Assert
        mockDownloadService.Verify(
            s => s.DownloadFile(-1, cancellationTokenSource.Token),
            Times.Once);
    }

    private void VerifyLog(LogLevel level, Times times)
    {
        mockLogger.Verify(
            logger => logger.Log(
                level,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }
}
