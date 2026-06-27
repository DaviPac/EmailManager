namespace EmailManager.Api.Email.DTOs;

public record MailboxResponse(Guid Id, string Address, DateTimeOffset CreatedAt);