using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Stub.Domain.Enums;
using Stub.Infrastructure.Providers.Abstractions;

namespace Stub.Infrastructure.Providers.Square;

public class SquarePosTransactionAdapter : IPosTransactionAdapter
{
    private readonly HttpClient _httpClient;
    private readonly SquareOptions _options;

    public SquarePosTransactionAdapter(HttpClient httpClient, IOptions<SquareOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public Provider Provider => Provider.Square;

    public async Task<string> FetchTransactionAsync(string externalTransactionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.AccessToken))
        {
            throw new InvalidOperationException("Square access token is not configured. Set Square__AccessToken via environment variables.");
        }

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"v2/payments/{Uri.EscapeDataString(externalTransactionId)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Square API returned {(int)response.StatusCode} when fetching transaction '{externalTransactionId}'. Body: {content}",
                null,
                response.StatusCode);
        }

        return content;
    }
}
