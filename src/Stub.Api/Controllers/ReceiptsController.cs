using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stub.Infrastructure.Persistence;

namespace Stub.Api.Controllers;

[ApiController]
[Route("api/receipts")]
public class ReceiptsController : ControllerBase
{
    private readonly StubDbContext _context;

    public ReceiptsController(StubDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetReceipts()
    {
        var receipts = await _context.Receipts
            .Include(r => r.ReceiptItems)
            .ToListAsync();

        return Ok(receipts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetReceipt(Guid id)
    {
        var receipt = await _context.Receipts
            .Include(r => r.ReceiptItems)
            .Include(r => r.ReceiptPayments)
            .Include(r => r.ReceiptAssignments)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (receipt == null)
        {
            return NotFound();
        }

        return Ok(receipt);
    }
}
