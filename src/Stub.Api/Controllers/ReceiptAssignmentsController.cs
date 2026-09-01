using Microsoft.AspNetCore.Mvc;
using Stub.Domain.Entities;
using Stub.Domain.Enums;
using Stub.Infrastructure.Persistence;

namespace Stub.Api.Controllers;

[ApiController]
[Route("api/receipt-assignments")]
public class ReceiptAssignmentsController : ControllerBase
{
    private readonly StubDbContext _context;

    public ReceiptAssignmentsController(StubDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateReceiptAssignment([FromBody] CreateReceiptAssignmentRequest request)
    {
        var receipt = await _context.Receipts.FindAsync(request.ReceiptId);
        if (receipt == null)
        {
            return NotFound("Receipt not found");
        }

        var receiptIdentity = await _context.ReceiptIdentities.FindAsync(request.ReceiptIdentityId);
        if (receiptIdentity == null)
        {
            return NotFound("Receipt identity not found");
        }

        var assignment = new ReceiptAssignment
        {
            Id = Guid.NewGuid(),
            ReceiptId = request.ReceiptId,
            ReceiptIdentityId = request.ReceiptIdentityId,
            AssignmentMethod = request.AssignmentMethod,
            AssignedAt = DateTimeOffset.UtcNow,
            Status = AssignmentStatus.Active
        };

        _context.ReceiptAssignments.Add(assignment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetReceiptAssignment), new { id = assignment.Id }, assignment);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetReceiptAssignment(Guid id)
    {
        var assignment = await _context.ReceiptAssignments.FindAsync(id);
        if (assignment == null)
        {
            return NotFound();
        }

        return Ok(assignment);
    }
}

public record CreateReceiptAssignmentRequest(
    Guid ReceiptId,
    Guid ReceiptIdentityId,
    AssignmentMethod AssignmentMethod);
