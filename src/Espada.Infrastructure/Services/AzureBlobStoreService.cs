using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Espada.Application.Contracts.Blobs;
using Espada.Application.Models;
using System.Security.Cryptography;

namespace Espada.Infrastructure.Services;

internal sealed class AzureBlobStoreService(BlobContainerClient container) : IBlobStoreService
{
    public AzureBlobStoreService(Uri containerUri) : this(new BlobContainerClient(containerUri, new DefaultAzureCredential()))
    {
    }

    public async Task<BlobDescriptor> PutAsync(Stream content, BlobWriteOptions options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.MediaType);

        string temporaryPath = Path.Join(Path.GetTempPath(), $"espada-blob-{Guid.NewGuid():N}.tmp");
        long length = 0;
        using IncrementalHash hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

        try
        {
            await using (FileStream temporary = new(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                byte[] buffer = new byte[81920];
                int read;
                while ((read = await content.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    hasher.AppendData(buffer, 0, read);
                    await temporary.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                    length += read;
                }
            }

            BlobHash hash = new(Convert.ToHexStringLower(hasher.GetHashAndReset()));
            BlobClient blob = container.GetBlobClient(hash.Value);

            await using FileStream upload = new(temporaryPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
            try
            {
                await blob.UploadAsync(
                    upload,
                    new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders { ContentType = options.MediaType },
                        Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All }
                    },
                    cancellationToken);
            }
            catch (RequestFailedException exception) when (exception.Status == 412)
            {
                // Content addressing makes an existing blob the successful idempotent result.
            }

            return new BlobDescriptor(hash, length, options.MediaType);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    public async Task<Stream> OpenReadAsync(BlobHash hash, CancellationToken cancellationToken)
    {
        BlobDownloadStreamingResult response = await container.GetBlobClient(hash.Value).DownloadStreamingAsync(cancellationToken: cancellationToken);
        return response.Content;
    }

    public async Task<bool> ExistsAsync(BlobHash hash, CancellationToken cancellationToken) =>
        (await container.GetBlobClient(hash.Value).ExistsAsync(cancellationToken)).Value;

    public async Task DeleteAsync(BlobHash hash, CancellationToken cancellationToken) =>
        _ = await container.GetBlobClient(hash.Value).DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
}