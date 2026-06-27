namespace EmailManager.Domain.Email;

public class Mailbox
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = "";
    public string Address { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public List<EmailMessage> Messages { get; set; } = new();
}