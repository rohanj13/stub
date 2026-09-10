using Microsoft.AspNetCore.Mvc;
using Stub.Domain.Entities;
using Stub.Domain.Enums;
using Stub.Infrastructure.Persistence;
using Stub.Infrastructure.Providers.Abstractions;

namespace Stub.Api.Controllers;

[ApiController]
[Route("api/webhooks/square")]
public class SquareWebhooksController : ControllerBase
{
    private readonly StubDbContext _context;
    private readonly IPosTransactionAdapterResolver _adapterResolver;

    public SquareWebhooksController(StubDbContext context, IPosTransactionAdapterResolver adapterResolver)
    {
        _context = context;
        _adapterResolver = adapterResolver;
    }

    [HttpPost]
    public async Task<IActionResult> HandleWebhook([FromBody] SquareWebhookPayload payload, CancellationToken cancellationToken)
    {
        var squareAdapter = _adapterResolver.GetRequiredAdapter(Provider.Square);
        string squareTransactionPayload;

        try
        {
            squareTransactionPayload = await squareAdapter.FetchTransactionAsync(payload.TransactionId, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(title: "Square integration is not configured", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
        catch (HttpRequestException ex)
        {
            return Problem(title: "Square API request failed", detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }

        // Create a raw transaction record
        var rawTransaction = new RawTransaction
        {
            Id = Guid.NewGuid(),
            MerchantId = payload.MerchantId,
            Provider = Provider.Square,
            ProviderEventType = "transaction.created",
            ExternalTransactionId = payload.TransactionId,
            Payload = squareTransactionPayload,
            ReceivedAt = DateTimeOffset.UtcNow,
            ProcessingStatus = ProcessingStatus.Pending
        };

        _context.RawTransactions.Add(rawTransaction);
        await _context.SaveChangesAsync(cancellationToken);

        // Note: In a real implementation, this would trigger async processing
        // to transform the raw transaction into a canonical receipt

        return Ok(new { rawTransactionId = rawTransaction.Id, status = "received" });
    }
}

public record SquareWebhookPayload(
    Guid MerchantId,
    string TransactionId,
    decimal Amount,
    string Currency);
