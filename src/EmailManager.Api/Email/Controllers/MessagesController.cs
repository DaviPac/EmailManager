using System.Security.Claims;
using EmailManager.Api.Email.DTOs;
using EmailManager.Application.Abstractions;
using EmailManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmailManager.Api.Email.Controllers;

[ApiController]
[Authorize]
public class MessagesController(AppDbContext db, IAttachmentStorage storage) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet("mailboxes/{mailboxId:guid}/messages")]
    public async Task<IActionResult> ListByMailbox(Guid mailboxId)
    {
        if (!await db.Mailboxes.AnyAsync(m => m.Id == mailboxId && m.UserId == UserId))
            return NotFound();

        return Ok(await db.Messages
            .Where(m => m.MailboxId == mailboxId)
            .OrderByDescending(m => m.ReceivedAt)
            .Select(m => new EmailMessageResponse(m.Id, m.From, m.Subject, m.IsRead, m.ReceivedAt))
            .ToListAsync());
    }

    [HttpGet("messages/{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var message = await db.Messages
            .Include(m => m.Attachments)
            .Where(m => m.Id == id &&
                db.Mailboxes.Any(b => b.Id == m.MailboxId && b.UserId == UserId))
            .FirstOrDefaultAsync();

        if (message is null) return NotFound();

        if (!message.IsRead) { message.IsRead = true; await db.SaveChangesAsync(); }

        return Ok(new EmailDetailsResponse(
            message.Id, message.From, message.To, message.Cc,
            message.Subject, message.TextBody, message.HtmlBody, message.IsRead,
            message.ReceivedAt,
            [.. message.Attachments.Select(a =>
                new AttachmentDto(
                    a.Id,
                    a.FileName,
                    a.ContentType,
                    a.SizeBytes
                ))]
        ));
    }

    [HttpDelete("messages/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var linhasAfetadas = await db.Messages
            .Where(m => m.Id == id)
            .ExecuteDeleteAsync();

        if (linhasAfetadas == 0)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("messages/{id:guid}/attachments/{attachmentId:guid}")]
    public async Task<IActionResult> Download(Guid id, Guid attachmentId)
    {
        var att = await db.Attachments.FirstOrDefaultAsync(a =>
            a.Id == attachmentId && a.EmailMessageId == id &&
            db.Messages.Any(m => m.Id == a.EmailMessageId &&
                db.Mailboxes.Any(b => b.Id == m.MailboxId && b.UserId == UserId)));

        if (att is null) return NotFound();

        var stream = await storage.OpenReadAsync(att.StoragePath, HttpContext.RequestAborted);
        return File(stream, att.ContentType, att.FileName);
    }
}