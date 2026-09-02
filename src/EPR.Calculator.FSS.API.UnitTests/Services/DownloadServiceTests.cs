using System.Net;
using System.Net.Http.Headers;
using EPR.Calculator.FSS.API.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Calculator.FSS.API.UnitTests.Services;

[TestClass]
public class DownloadServiceTests
{
    private readonly Mock<ILogger<DownloadService>> mockLogger = new();

    [TestMethod]
    public async Task DownloadFile_WhenFileExists_ReturnsStreamedContentWithHeadersFromResponse()
    {
        // Arrange
        const int runId = 42;
        var expectedBytes = "{\"field\":\"value\"}"u8.ToArray();

        var handler = new FakeHttpMessageHandler(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(expectedBytes),
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "billing-42.json",
            };
            return response;
        });

        var service = CreateService(handler);

        // Act
        var result = await service.DownloadFile(runId, CancellationToken.None);

        // Assert
        Assert.AreEqual("application/json", result.ContentType);
        Assert.AreEqual("billing-42.json", result.FileDownloadName);

        using var reader = new StreamReader(result.FileStream);
        var content = await reader.ReadToEndAsync();
        Assert.AreEqual("{\"field\":\"value\"}", content);

        Assert.IsNotNull(handler.LastRequest);
        Assert.AreEqual(HttpMethod.Get, handler.LastRequest.Method);
        Assert.AreEqual(
            new Uri($"https://calculator-api.test/v1/downloadBillingJson/{runId}"),
            handler.LastRequest.RequestUri);
    }

    [TestMethod]
    public async Task DownloadFile_WhenFileNotFound_ThrowsFileNotFoundException()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var service = CreateService(handler);

        // Act / Assert
        await Assert.ThrowsExactlyAsync<FileNotFoundException>(
            () => service.DownloadFile(99, CancellationToken.None));
    }

    [TestMethod]
    public async Task DownloadFile_WhenServerError_ThrowsHttpRequestException()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var service = CreateService(handler);

        // Act / Assert
        await Assert.ThrowsExactlyAsync<HttpRequestException>(
            () => service.DownloadFile(1, CancellationToken.None));
    }

    [TestMethod]
    public async Task DownloadFile_WhenNoContentDisposition_FallsBackToDefaultFileName()
    {
        // Arrange
        const int runId = 7;

        var handler = new FakeHttpMessageHandler(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent("{}"u8.ToArray()),
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return response;
        });

        var service = CreateService(handler);

        // Act
        var result = await service.DownloadFile(runId, CancellationToken.None);

        // Assert
        Assert.AreEqual($"billing-{runId}.json", result.FileDownloadName);
    }

    private DownloadService CreateService(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://calculator-api.test/"),
        };

        return new DownloadService(httpClient, mockLogger.Object);
    }

    private sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(responder(request));
        }
    }
}
