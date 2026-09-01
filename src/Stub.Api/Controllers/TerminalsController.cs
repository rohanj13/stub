using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stub.Domain.Entities;
using Stub.Domain.Enums;
using Stub.Infrastructure.Persistence;

namespace Stub.Api.Controllers;

[ApiController]
[Route("api/terminals")]
public class TerminalsController : ControllerBase
{
    private readonly StubDbContext _context;

    public TerminalsController(StubDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTerminal([FromBody] CreateTerminalRequest request)
    {
        var merchant = await _context.Merchants.FindAsync(request.MerchantId);
        if (merchant == null)
        {
            return NotFound("Merchant not found");
        }

        var terminal = new Terminal
        {
            Id = Guid.NewGuid(),
            MerchantId = request.MerchantId,
            Name = request.Name,
            ExternalTerminalId = request.ExternalTerminalId,
            Status = TerminalStatus.Active
        };

        _context.Terminals.Add(terminal);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTerminal), new { id = terminal.Id }, terminal);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTerminal(Guid id)
    {
        var terminal = await _context.Terminals.FindAsync(id);
        if (terminal == null)
        {
            return NotFound();
        }

        return Ok(terminal);
    }
}

public record CreateTerminalRequest(Guid MerchantId, string Name, string ExternalTerminalId);
