using System.Net;
using System.Text;
using EPR.Calculator.FSS.API.Clients.EprCommonDataApi;
using EPR.Calculator.FSS.API.Configs;
using EPR.Calculator.FSS.API.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.FSS.API.UnitTests.Clients.EprCommonDataApi;

[TestClass]
public class EprCommonDataApiHttpClientTests
{
    private const string ResponseJson =
        """
        {
          "organisationsDetails": [
            {
              "organisationId": "12345",
              "organisationName": "Example Organisation Ltd",
              "organisationTradingName": "Example Trading Name",
              "financialYear": "2025-26",
              "approvedDate": "2025-06-01T10:30:00Z",
              "companiesHouseNumber": "12345678",
              "homeNationCode": "ENG",
              "serviceOfNoticeAddrLine1": "123 Example Street",
              "serviceOfNoticeAddrLine2": null,
              "serviceOfNoticeAddrCity": "London",
              "serviceOfNoticeAddrCounty": null,
              "serviceOfNoticeAddrCountry": "United Kingdom",
              "serviceOfNoticeAddrPostcode": "E1 6AN",
              "serviceOfNoticeAddrPhoneNumber": null,
              "soleTraderFirstName": null,
              "soleTraderLastName": null,
              "soleTraderPhoneNumber": null,
              "soleTraderEmail": null,
              "primaryContactPersonFirstName": "Jane",
              "primaryContactPersonLastName": "Smith",
              "primaryContactPersonPhoneNumber": "+44 7911 654321",
              "primaryContactPersonEmail": "jane.smith@example.com",
              "subsidiaryDetails": [
                {
                  "subsidiaryId": "900001",
                  "subsidiaryName": "Happy Shopper",
                  "subsidiaryTradingName": null,
                  "financialYear": "2025-26",
                  "approvedDate": "2025-06-02T00:00:00Z"
                }
              ]
            }
          ]
        }
        """;

    [TestMethod]
    public async Task GetBillingOrganisations_ReturnsOrganisationsFromCommonDataApi()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(_ => JsonResponse(ResponseJson));
        var client = CreateClient(handler);

        // Act
        var result = await client.GetBillingOrganisations(approvedAfter: null, financialYear: null, CancellationToken.None);

        // Assert
        Assert.AreEqual(1, result.Count);

        var organisation = result.Single();
        Assert.AreEqual("12345", organisation.OrganisationId);
        Assert.AreEqual("Example Organisation Ltd", organisation.OrganisationName);
        Assert.AreEqual("2025-26", organisation.FinancialYear);
        Assert.AreEqual(new DateTime(2025, 6, 1, 10, 30, 0, DateTimeKind.Utc), organisation.ApprovedDate);
        Assert.AreEqual(DateTimeKind.Utc, organisation.ApprovedDate.Kind);
        Assert.AreEqual("jane.smith@example.com", organisation.PrimaryContactPersonEmail);
        Assert.IsNull(organisation.SoleTraderEmail);

        var subsidiary = organisation.SubsidiaryDetails.Single();
        Assert.AreEqual("900001", subsidiary.SubsidiaryId);
        Assert.AreEqual("Happy Shopper", subsidiary.SubsidiaryName);
        Assert.AreEqual(new DateTime(2025, 6, 2, 0, 0, 0, DateTimeKind.Utc), subsidiary.ApprovedDate);
    }

    [TestMethod]
    public async Task GetBillingOrganisations_WithoutFilters_CallsEndpointWithoutQueryString()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(_ => JsonResponse("""{"organisationsDetails":[]}"""));
        var client = CreateClient(handler);

        // Act
        var result = await client.GetBillingOrganisations(approvedAfter: null, financialYear: null, CancellationToken.None);

        // Assert
        Assert.AreEqual(0, result.Count);
        Assert.IsNotNull(handler.LastRequest);
        Assert.AreEqual(HttpMethod.Get, handler.LastRequest.Method);
        Assert.AreEqual(
            new Uri("https://common-data-api.test/api/paycal/fss/v1/billing-organisations"),
            handler.LastRequest.RequestUri);
    }

    [TestMethod]
    public async Task GetBillingOrganisations_WithFilters_PassesThemAsQueryParameters()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(_ => JsonResponse("""{"organisationsDetails":[]}"""));
        var client = CreateClient(handler);

        // Act
        await client.GetBillingOrganisations(
            approvedAfter: new DateTimeOffset(2025, 6, 1, 9, 0, 0, TimeSpan.FromHours(1)),
            financialYear: new FinancialYear("2025-26"),
            CancellationToken.None);

        // Assert
        Assert.IsNotNull(handler.LastRequest?.RequestUri);
        Assert.AreEqual("/api/paycal/fss/v1/billing-organisations", handler.LastRequest.RequestUri.AbsolutePath);

        var query = QueryHelpers.ParseQuery(handler.LastRequest.RequestUri.Query);
        Assert.AreEqual("2025-26", query["financialYear"].ToString());
        Assert.AreEqual("2025-06-01T08:00:00.0000000Z", query["approvedAfter"].ToString());
    }

    [TestMethod]
    public async Task GetBillingOrganisations_WhenCommonDataApiReturnsError_ThrowsHttpRequestException()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var client = CreateClient(handler);

        // Act / Assert
        await Assert.ThrowsExactlyAsync<HttpRequestException>(
            () => client.GetBillingOrganisations(approvedAfter: null, financialYear: null, CancellationToken.None));
    }

    private static HttpResponseMessage JsonResponse(string json) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };

    private static EprCommonDataApiHttpClient CreateClient(HttpMessageHandler handler) =>
        new(
            new HttpClient(handler),
            Options.Create(new EprCommonDataApiSettings { BaseUrl = "https://common-data-api.test/" }));

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
