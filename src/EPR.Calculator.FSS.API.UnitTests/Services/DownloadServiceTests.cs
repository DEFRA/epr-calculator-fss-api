using System.Net;
using System.Net.Http.Headers;
using EPR.Calculator.FSS.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
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
        var content = await reader.ReadToEndAsync(CancellationToken.None);
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

    [TestMethod]
    public async Task DownloadFile_StreamForwardsToUnderlyingContentAndDisposesItOnDispose()
    {
        var innerStream = new MemoryStream("hello world"u8.ToArray());

        var handler = new FakeHttpMessageHandler(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(innerStream),
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return response;
        });

        var service = CreateService(handler);

        var result = await service.DownloadFile(1, CancellationToken.None);
        var stream = result.FileStream;

        Assert.IsTrue(stream.CanRead);
        Assert.IsTrue(stream.CanSeek);
        Assert.IsFalse(stream.CanWrite);
        Assert.AreEqual(innerStream.Length, stream.Length);

        stream.Position = 0;
        Assert.AreEqual(0, stream.Position);

#pragma warning disable S6966
        var buffer = new byte[5];
        var bytesRead = stream.Read(buffer, 0, buffer.Length);
        Assert.AreEqual(5, bytesRead);
        Assert.AreEqual("hello", System.Text.Encoding.UTF8.GetString(buffer));

        Assert.AreEqual(6, stream.Seek(6, SeekOrigin.Begin));

        stream.Flush();

        var writeBuffer = "WORLD"u8.ToArray();
        Assert.ThrowsExactly<NotSupportedException>(() => stream.Write(writeBuffer, 0, writeBuffer.Length));
#pragma warning restore S6966

        Assert.ThrowsExactly<NotSupportedException>(() => stream.SetLength(stream.Length));

        await stream.DisposeAsync();

        Assert.IsFalse(innerStream.CanRead, "disposing the wrapper should dispose the underlying content stream");
    }

    [TestMethod]
    public async Task DownloadFile_WhenCallerAcceptsGzip_AsksForGzipAndPassesTheCompressedResponseThrough()
    {
        var compressed = new byte[] { 0x1f, 0x8b, 0x08, 0x00, 0x01, 0x02, 0x03 };
        var handler = new FakeHttpMessageHandler(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(compressed) };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            response.Content.Headers.ContentEncoding.Add("gzip");
            return response;
        });
        var service = CreateService(handler, callerAcceptEncoding: "gzip, deflate, br");

        var result = await service.DownloadFile(1, CancellationToken.None);
        var (headers, body) = await Execute(result);

        Assert.AreEqual("gzip", handler.LastRequest!.Headers.AcceptEncoding.Single().Value);
        Assert.AreEqual("gzip", headers.ContentEncoding.ToString());
        Assert.AreEqual("Accept-Encoding", headers.Vary.ToString());
        CollectionAssert.AreEqual(compressed, body);
    }

    [TestMethod]
    public async Task DownloadFile_WhenCallerDoesNotAcceptGzip_DoesNotAskForIt()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent("{}"u8.ToArray()) });
        var service = CreateService(handler, callerAcceptEncoding: "br");

        var result = await service.DownloadFile(1, CancellationToken.None);
        var (headers, body) = await Execute(result);

        Assert.AreEqual(0, handler.LastRequest!.Headers.AcceptEncoding.Count);
        Assert.AreEqual(0, headers.ContentEncoding.Count);
        CollectionAssert.AreEqual("{}"u8.ToArray(), body);
    }

    [TestMethod]
    public async Task DownloadFile_WithoutACaller_DoesNotAskForCompression()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent("{}"u8.ToArray()) });
        var service = CreateService(handler);

        await service.DownloadFile(1, CancellationToken.None);

        Assert.AreEqual(0, handler.LastRequest!.Headers.AcceptEncoding.Count);
    }

    private DownloadService CreateService(HttpMessageHandler handler, string? callerAcceptEncoding = null)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://calculator-api.test/"),
        };

        var httpContextAccessor = new HttpContextAccessor();
        if (callerAcceptEncoding is not null)
        {
            httpContextAccessor.HttpContext = new DefaultHttpContext();
            httpContextAccessor.HttpContext.Request.Headers.AcceptEncoding = callerAcceptEncoding;
        }

        return new DownloadService(httpClient, httpContextAccessor, mockLogger.Object);
    }

    private static async Task<(IHeaderDictionary Headers, byte[] Body)> Execute(FileStreamResult result)
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().AddLogging().AddMvcCore().Services.BuildServiceProvider(),
        };
        var body = new MemoryStream();
        httpContext.Response.Body = body;

        await result.ExecuteResultAsync(new ActionContext(httpContext, new RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()));

        return (httpContext.Response.Headers, body.ToArray());
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
