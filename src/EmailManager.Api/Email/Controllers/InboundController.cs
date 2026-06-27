using System.Text.Json;
using EmailManager.Api.Hubs;
using EmailManager.Application.Abstractions;
using EmailManager.Domain.Email;
using EmailManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EmailManager.Api.Email.Controllers;

[ApiController]
[Route("webhooks/sendgrid")]
public class InboundController(
    AppDbContext db, IConfiguration config, IAttachmentStorage storage,
    IHubContext<NotificationsHub> hub) : ControllerBase
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

        var recipients = envelope?.To.Select(a => a.ToLowerInvariant()) ?? [];

        var mailboxes = await db.Mailboxes
            .Where(m => recipients.Contains(m.Address))
            .ToListAsync();

        if (mailboxes.Count == 0)
            return Ok();

        var saved = new List<(string UserId, EmailMessage Msg)>();

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
            saved.Add((mailbox.UserId, message));

            foreach (var (userId, msg) in saved)
            {
                await hub.Clients.User(userId).SendAsync("newMessage", new
                {
                    msg.Id, msg.MailboxId, msg.From, msg.Subject, msg.ReceivedAt
                });
            }
        }

        await db.SaveChangesAsync();
        return Ok();
    }
}