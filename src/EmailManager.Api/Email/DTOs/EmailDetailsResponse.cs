namespace EmailManager.Api.Email.DTOs;

public record EmailDetailsResponse(
    Guid Id,
    string From,
    string To,
    string? Cc,
    string Subject,
    string? TextBody,
    string? HtmlBody,
    bool IsRead,
    DateTimeOffset ReceivedAt,
    List<AttachmentDto> Attachments);