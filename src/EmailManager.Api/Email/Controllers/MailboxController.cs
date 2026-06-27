using System.Security.Claims;
using EmailManager.Api.Email.DTOs;
using EmailManager.Domain.Email;
using EmailManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmailManager.Api.Email.Controllers;

[ApiController]
[Authorize]
public class MailboxesController(AppDbContext db, IConfiguration config) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpPost("mailboxes")]
    public async Task<IActionResult> Create(CreateMailboxRequest req)
    {
        var address = $"{req.LocalPart.Trim().ToLowerInvariant()}@{config["Domain"]}";

        if (await db.Mailboxes.AnyAsync(m => m.Address == address))
            return Conflict("Esse endereço já está em uso.");

        var mailbox = new Mailbox { UserId = UserId, Address = address };
        db.Mailboxes.Add(mailbox);
        await db.SaveChangesAsync();

        return Ok(new MailboxResponse(mailbox.Id, mailbox.Address, mailbox.CreatedAt));
    }

    [HttpGet("mailboxes")]
    public async Task<IActionResult> List() =>
        Ok(await db.Mailboxes
            .Where(m => m.UserId == UserId)
            .Select(m => new MailboxResponse(m.Id, m.Address, m.CreatedAt))
            .ToListAsync());

    [HttpDelete("mailboxes/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var linhasAfetadas = await db.Mailboxes
            .Where(m => m.UserId == UserId)
            .Where(m => m.Id == id)
            .ExecuteDeleteAsync();

        if (linhasAfetadas == 0)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpHead("mailboxes/check-address")]
    public async Task<IActionResult> CheckAddressHead([FromQuery] string address)
    {
        var fullAddress =  $"{address.Trim().ToLowerInvariant()}@{config["Domain"]}";

        var exists = await db.Mailboxes.AnyAsync(m => m.Address == fullAddress);

        if (exists)
        {
            return Ok();
        }

        return NotFound();
    }
}