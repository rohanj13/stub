using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stub.Domain.Entities;
using Stub.Domain.Enums;
using Stub.Infrastructure.Persistence;

namespace Stub.Api.Controllers;

[ApiController]
[Route("api/receipt-identities")]
public class ReceiptIdentitiesController : ControllerBase
{
    private readonly StubDbContext _context;

    public ReceiptIdentitiesController(StubDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateReceiptIdentity()
    {
        var receiptIdentity = new ReceiptIdentity
        {
            Id = Guid.NewGuid(),
            PublicIdentifier = $"rid_{Guid.NewGuid():N}".Substring(0, 20),
            Status = ReceiptIdentityStatus.Active
        };

        _context.ReceiptIdentities.Add(receiptIdentity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetReceiptIdentity), new { id = receiptIdentity.Id }, receiptIdentity);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetReceiptIdentity(Guid id)
    {
        var receiptIdentity = await _context.ReceiptIdentities.FindAsync(id);
        if (receiptIdentity == null)
        {
            return NotFound();
        }

        return Ok(receiptIdentity);
    }

    [HttpGet("{publicIdentifier}/receipts")]
    public async Task<IActionResult> GetReceiptsByIdentity(string publicIdentifier)
    {
        var receiptIdentity = await _context.ReceiptIdentities
            .FirstOrDefaultAsync(ri => ri.PublicIdentifier == publicIdentifier);

        if (receiptIdentity == null)
        {
            return NotFound();
        }

        var receipts = await _context.ReceiptAssignments
            .Where(ra => ra.ReceiptIdentityId == receiptIdentity.Id)
            .Include(ra => ra.Receipt)
            .ThenInclude(r => r.ReceiptItems)
            .Select(ra => ra.Receipt)
            .ToListAsync();

        return Ok(receipts);
    }
}
