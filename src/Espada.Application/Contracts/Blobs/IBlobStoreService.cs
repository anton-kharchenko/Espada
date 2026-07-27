using Espada.Application.Models;

namespace Espada.Application.Contracts.Blobs;

public interface IBlobStoreService
{
    Task<BlobDescriptor> PutAsync(Stream content, BlobWriteOptions options, CancellationToken cancellationToken);

    Task<Stream> OpenReadAsync(BlobHash hash, CancellationToken cancellationToken);

    Task DeleteAsync(BlobHash hash, CancellationToken cancellationToken);
}