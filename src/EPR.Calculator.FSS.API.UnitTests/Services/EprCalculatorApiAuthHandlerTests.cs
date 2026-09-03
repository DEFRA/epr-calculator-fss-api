using System.Net;
using Azure.Core;
using EPR.Calculator.FSS.API.Configs;
using EPR.Calculator.FSS.API.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace EPR.Calculator.FSS.API.UnitTests.Services;

[TestClass]
public class EprCalculatorApiAuthHandlerTests
{
    [TestMethod]
    public async Task SendAsync_AttachesBearerTokenFromCredential_ForConfiguredScope()
    {
        // Arrange
        const string expectedToken = "test-access-token";
        const string expectedScope = "api://calculator-api/.default";

        TokenRequestContext? capturedContext = null;

        var mockCredential = new Mock<TokenCredential>();
        mockCredential
            .Setup(c => c.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()))
            .Callback<TokenRequestContext, CancellationToken>((context, _) => capturedContext = context)
            .ReturnsAsync(new AccessToken(expectedToken, DateTimeOffset.UtcNow.AddHours(1)));

        var options = Options.Create(new EprCalculatorApiSettings { BaseUrl = "https://calculator-api.test", Scope = expectedScope });

        var innerHandler = new FakeInnerHandler();
        var handler = new EprCalculatorApiAuthHandler(mockCredential.Object, options)
        {
            InnerHandler = innerHandler,
        };

        using var invoker = new HttpMessageInvoker(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://calculator-api.test/v1/downloadBillingJson/1");

        // Act
        using var response = await invoker.SendAsync(request, CancellationToken.None);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        Assert.IsNotNull(innerHandler.LastRequest);
        Assert.AreEqual("Bearer", innerHandler.LastRequest.Headers.Authorization?.Scheme);
        Assert.AreEqual(expectedToken, innerHandler.LastRequest.Headers.Authorization?.Parameter);

        Assert.IsNotNull(capturedContext);
        CollectionAssert.AreEqual(new[] { expectedScope }, capturedContext.Value.Scopes);
    }

    private sealed class FakeInnerHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
