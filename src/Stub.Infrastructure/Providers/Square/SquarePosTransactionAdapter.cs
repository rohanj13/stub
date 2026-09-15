using System.Text.Json;
using Microsoft.Extensions.Options;
using Square;
using Square.Payments;
using Stub.Domain.Enums;
using Stub.Infrastructure.Providers.Abstractions;

namespace Stub.Infrastructure.Providers.Square;

public class SquarePosTransactionAdapter : IPosTransactionAdapter
{
    private readonly ISquareClient _squareClient;
    private readonly SquareOptions _options;

    public SquarePosTransactionAdapter(ISquareClient squareClient, IOptions<SquareOptions> options)
    {
        _squareClient = squareClient;
        _options = options.Value;
    }

    public Provider Provider => Provider.Square;

    public async Task<string> FetchTransactionAsync(string externalTransactionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.AccessToken))
        {
            throw new InvalidOperationException("Square access token is not configured. Set Square__AccessToken via environment variables.");
        }

        var response = await _squareClient.Payments.GetAsync(
            new GetPaymentsRequest { PaymentId = externalTransactionId },
            cancellationToken: cancellationToken);

        if (response.Payment is null)
        {
            throw new InvalidOperationException($"Square payment '{externalTransactionId}' was not returned by the API.");
        }

        return JsonSerializer.Serialize(response.Payment);
    }
}
