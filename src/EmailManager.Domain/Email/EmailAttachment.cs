namespace EmailManager.Domain.Email;

public class EmailAttachment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmailMessageId { get; set; }
    public string FileName { get; set; } = "";
    public string ContentType { get; set; } = "";
    public long SizeBytes { get; set; }
    public string StoragePath { get; set; } = "";
}