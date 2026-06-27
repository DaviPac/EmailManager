namespace EmailManager.Application.Abstractions;

public interface IAttachmentStorage
{
    Task<string> SaveAsync(Guid messageId, string fileName, Stream content, CancellationToken ct = default);
    Task<Stream> OpenReadAsync(string storagePath, CancellationToken ct = default);
}