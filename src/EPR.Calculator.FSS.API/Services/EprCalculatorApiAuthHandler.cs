// Pulled from the Azure.Core assembly (aliased in the .csproj), not Azure.Identity - see the
// comment on the Azure.Core PackageReference for why. Don't add a plain `using Azure.Identity;`
// or a direct Azure.Identity PackageReference here; it reintroduces the Synapse auth regression.
extern alias AzureCoreCredentials;

using System.Net.Http.Headers;
using EPR.Calculator.FSS.API.Configs;
using Microsoft.Extensions.Options;
using TokenCredential = AzureCoreCredentials::Azure.Core.TokenCredential;
using TokenRequestContext = AzureCoreCredentials::Azure.Core.TokenRequestContext;

namespace EPR.Calculator.FSS.API.Services;

/// <summary>
/// Attaches an Entra ID bearer token, acquired via the app's Managed Identity, to outgoing requests
/// to the EPR Calculator API.
/// </summary>
/// <param name="credential">The token credential used to acquire tokens (Managed Identity in Azure).</param>
/// <param name="eprCalculatorApiSettings">Settings containing the scope to request a token for.</param>
public class EprCalculatorApiAuthHandler(
    TokenCredential credential,
    IOptions<EprCalculatorApiSettings> eprCalculatorApiSettings)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var tokenRequestContext = new TokenRequestContext([eprCalculatorApiSettings.Value.Scope]);
        var token = await credential.GetTokenAsync(tokenRequestContext, cancellationToken);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

        return await base.SendAsync(request, cancellationToken);
    }
}
