using Espada.Application.Models;
using Espada.Infrastructure.Services;

namespace Espada.Tests.Infrastructure.Ingestion;

public sealed class FileSystemBlobStoreServiceTests
{
    [Fact]
    public async Task PutSameContentTwice_ShouldReuseSha256Blob()
    {
        string root = Path.Join(Path.GetTempPath(), $"espada-blobs-{Guid.NewGuid():N}");
        try
        {
            FileSystemBlobStoreService storeService = new(root);
            byte[] payload = "same content"u8.ToArray();

            await using MemoryStream firstContent = new(payload);
            BlobDescriptor first = await storeService.PutAsync(firstContent, new BlobWriteOptions("text/plain"), TestContext.Current.CancellationToken);
            await using MemoryStream secondContent = new(payload);
            BlobDescriptor second = await storeService.PutAsync(secondContent, new BlobWriteOptions("text/plain"), TestContext.Current.CancellationToken);

            Assert.Equal(first.Hash, second.Hash);
            Assert.True(await storeService.ExistsAsync(first.Hash, TestContext.Current.CancellationToken));
            await using Stream content = await storeService.OpenReadAsync(first.Hash, TestContext.Current.CancellationToken);
            using StreamReader reader = new(content);
            Assert.Equal("same content", await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }
}