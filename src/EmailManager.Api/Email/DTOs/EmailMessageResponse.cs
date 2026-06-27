namespace EmailManager.Api.Email.DTOs;

public record EmailMessageResponse(
    Guid Id,
    string From,
    string Subject,
    bool IsRead,
    DateTimeOffset ReceivedAt);