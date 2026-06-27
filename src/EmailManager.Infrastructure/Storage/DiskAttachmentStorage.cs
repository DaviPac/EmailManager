using EmailManager.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace EmailManager.Infrastructure.Storage;

public class DiskAttachmentStorage(IOptions<AttachmentStorageOptions> options) : IAttachmentStorage
{
    private readonly string _root = options.Value.RootPath;

    public async Task<string> SaveAsync(
        Guid messageId, string fileName, Stream content, CancellationToken ct = default)
    {
        var relativePath = Path.Combine(messageId.ToString(), fileName);
        var fullPath = Path.Combine(_root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await using var file = File.Create(fullPath);
        await content.CopyToAsync(file, ct);
        return relativePath;
    }

    public Task<Stream> OpenReadAsync(string storagePath, CancellationToken ct = default) =>
        Task.FromResult<Stream>(File.OpenRead(Path.Combine(_root, storagePath)));
}