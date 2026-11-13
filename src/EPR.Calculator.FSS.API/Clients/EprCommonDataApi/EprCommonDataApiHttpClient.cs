using System.Globalization;
using EPR.Calculator.FSS.API.Configs;
using EPR.Calculator.FSS.API.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.FSS.API.Clients.EprCommonDataApi;

public interface IEprCommonDataApiClient
{
    /// <summary>
    /// Gets the organisation billing details from common-data-api's FSS billing organisations endpoint.
    /// </summary>
    /// <param name="approvedAfter">Only return organisations approved on or after this date.</param>
    /// <param name="financialYear">Only return organisations for this financial year.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>Organisation details collection.</returns>
    Task<IReadOnlyCollection<OrganisationDetails>> GetBillingOrganisations(
        DateTimeOffset? approvedAfter,
        FinancialYear? financialYear,
        CancellationToken cancellationToken);
}

public class EprCommonDataApiHttpClient(
    HttpClient httpClient,
    IOptions<EprCommonDataApiSettings> eprCommonDataApiSettings)
    : IEprCommonDataApiClient
{
    public async Task<IReadOnlyCollection<OrganisationDetails>> GetBillingOrganisations(
        DateTimeOffset? approvedAfter,
        FinancialYear? financialYear,
        CancellationToken cancellationToken)
    {
        var query = new Dictionary<string, string?>();

        if (financialYear is { } year)
        {
            query["financialYear"] = year.Value;
        }

        if (approvedAfter is { } after)
        {
            query["approvedAfter"] = after.UtcDateTime.ToString("O", CultureInfo.InvariantCulture);
        }

        var requestUri = QueryHelpers.AddQueryString(
            $"{eprCommonDataApiSettings.Value.BaseUrl.TrimEnd('/')}/api/paycal/fss/v1/billing-organisations",
            query);

        // Deserialised into FSS's own models rather than streamed through so FSS owns its response contract (~8 MB in prod).
        var response = await httpClient.GetFromJsonAsync<OrganisationsDetailsResponse>(requestUri, cancellationToken)
            ?? throw new InvalidOperationException("common-data-api returned an empty billing organisations response.");

        return response.OrganisationsDetails.ToList().AsReadOnly();
    }
}
