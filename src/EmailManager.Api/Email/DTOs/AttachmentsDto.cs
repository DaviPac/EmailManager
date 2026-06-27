namespace EmailManager.Api.Email.DTOs;

public record AttachmentDto(Guid Id, string FileName, string ContentType, long SizeBytes);