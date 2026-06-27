namespace EmailManager.Domain.Email;

public class EmailMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MailboxId { get; set; }
    public string From { get; set; } = "";
    public string To { get; set; } = "";
    public string? Cc { get; set; }
    public string Subject { get; set; } = "";
    public string? TextBody { get; set; }
    public string? HtmlBody { get; set; }
    public string? RawHeaders { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
    public List<EmailAttachment> Attachments { get; set; } = new();
}