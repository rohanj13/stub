using Microsoft.AspNetCore.Mvc;
using Stub.Domain.Entities;
using Stub.Domain.Enums;
using Stub.Infrastructure.Persistence;

namespace Stub.Api.Controllers;

[ApiController]
[Route("api/receipt-assignment-sessions")]
public class ReceiptAssignmentSessionsController : ControllerBase
{
    private readonly StubDbContext _context;

    public ReceiptAssignmentSessionsController(StubDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
    {
        var terminal = await _context.Terminals.FindAsync(request.TerminalId);
        if (terminal == null)
        {
            return NotFound("Terminal not found");
        }

        var receiptIdentity = await _context.ReceiptIdentities.FindAsync(request.ReceiptIdentityId);
        if (receiptIdentity == null)
        {
            return NotFound("Receipt identity not found");
        }

        var session = new ReceiptAssignmentSession
        {
            Id = Guid.NewGuid(),
            TerminalId = request.TerminalId,
            ReceiptIdentityId = request.ReceiptIdentityId,
            StartedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
            Status = AssignmentSessionStatus.Active
        };

        _context.ReceiptAssignmentSessions.Add(session);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSession), new { id = session.Id }, session);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSession(Guid id)
    {
        var session = await _context.ReceiptAssignmentSessions.FindAsync(id);
        if (session == null)
        {
            return NotFound();
        }

        return Ok(session);
    }
}

public record CreateSessionRequest(Guid TerminalId, Guid ReceiptIdentityId);
