using System.Text.Json;
using EmailManager.Application.Abstractions;
using EmailManager.Domain.Email;
using EmailManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmailManager.Api.Email.Controllers;

[ApiController]
[Route("webhooks/sendgrid")]
public class InboundController(
    AppDbContext db, IConfiguration config, IAttachmentStorage storage) : ControllerBase
{
    private record Envelope(string[] To, string From);

    [HttpPost("inbound")]
    [RequestSizeLimit(30_000_000)]
    public async Task<IActionResult> Inbound([FromQuery] string token)
    {
        if (token != config["SendGrid:InboundToken"])
            return Unauthorized();

        var form = await Request.ReadFormAsync();

        var envelope = JsonSerializer.Deserialize<Envelope>(
            form["envelope"].ToString(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var recipients = envelope?.To ?? [];

        var mailboxes = await db.Mailboxes
            .Where(m => recipients.Contains(m.Address))
            .ToListAsync();

        if (mailboxes.Count == 0)
            return Ok();

        foreach (var mailbox in mailboxes)
        {
            var message = new EmailMessage
            {
                MailboxId  = mailbox.Id,
                From       = form["from"].ToString(),
                To         = form["to"].ToString(),
                Cc         = form["cc"].ToString(),
                Subject    = form["subject"].ToString(),
                TextBody   = form["text"].ToString(),
                HtmlBody   = form["html"].ToString(),
                RawHeaders = form["headers"].ToString(),
            };

            foreach (var file in form.Files)
            {
                await using var stream = file.OpenReadStream();
                var relativePath = await storage.SaveAsync(
                    message.Id, file.FileName, stream, HttpContext.RequestAborted);

                message.Attachments.Add(new EmailAttachment
                {
                    FileName    = file.FileName,
                    ContentType = file.ContentType,
                    SizeBytes   = file.Length,
                    StoragePath = relativePath,
                });
            }

            db.Messages.Add(message);
        }

        await db.SaveChangesAsync();
        return Ok();
    }
}