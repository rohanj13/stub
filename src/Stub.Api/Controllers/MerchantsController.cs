using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stub.Domain.Entities;
using Stub.Infrastructure.Persistence;

namespace Stub.Api.Controllers;

[ApiController]
[Route("api/merchants")]
public class MerchantsController : ControllerBase
{
    private readonly StubDbContext _context;

    public MerchantsController(StubDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateMerchant([FromBody] CreateMerchantRequest request)
    {
        var merchant = new Merchant
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            LegalName = request.LegalName,
            Abn = request.Abn
        };

        _context.Merchants.Add(merchant);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMerchant), new { id = merchant.Id }, merchant);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMerchant(Guid id)
    {
        var merchant = await _context.Merchants.FindAsync(id);
        if (merchant == null)
        {
            return NotFound();
        }

        return Ok(merchant);
    }

    [HttpGet]
    public async Task<IActionResult> GetMerchants()
    {
        var merchants = await _context.Merchants.ToListAsync();
        return Ok(merchants);
    }
}

public record CreateMerchantRequest(string Name, string LegalName, string Abn);
