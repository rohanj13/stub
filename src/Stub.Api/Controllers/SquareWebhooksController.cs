using Microsoft.AspNetCore.Mvc;
using Stub.Domain.Entities;
using Stub.Domain.Enums;
using Stub.Infrastructure.Persistence;

namespace Stub.Api.Controllers;

[ApiController]
[Route("api/webhooks/square")]
public class SquareWebhooksController : ControllerBase
{
    private readonly StubDbContext _context;

    public SquareWebhooksController(StubDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> HandleWebhook([FromBody] SquareWebhookPayload payload)
    {
        // Create a raw transaction record
        var rawTransaction = new RawTransaction
        {
            Id = Guid.NewGuid(),
            MerchantId = payload.MerchantId,
            Provider = Provider.Square,
            ProviderEventType = "transaction.created",
            ExternalTransactionId = payload.TransactionId,
            Payload = System.Text.Json.JsonSerializer.Serialize(payload),
            ReceivedAt = DateTimeOffset.UtcNow,
            ProcessingStatus = ProcessingStatus.Pending
        };

        _context.RawTransactions.Add(rawTransaction);
        await _context.SaveChangesAsync();

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
