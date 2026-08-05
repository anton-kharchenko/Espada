using Espada.Application.Enums;
using Espada.Application.Exceptions;
using Espada.Application.Models;
using Espada.Domain.ValueObjects.SourceDefinitions;
using Espada.Infrastructure.Ingestion;
using Espada.Infrastructure.Options;
using Espada.Infrastructure.Services;
using Espada.Tests.Infrastructure.Ingestion.Fakes;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Espada.Tests.Infrastructure.Ingestion
{
    public sealed class SourceReaderSecurityTests
    {
        [Fact]
        public async Task ReadFile_OutsideConfiguredRoot_ShouldFailPermanently()
        {
            string allowedRoot = CreateTemporaryDirectory();
            string blobRoot = CreateTemporaryDirectory();
            try
            {
                SourceReader reader = CreateReader(allowedRoot, blobRoot);
                FileSourceDefinition definition = new(
                    Path.Join(Path.GetTempPath(), "outside.txt"),
                    null,
                    "outside.txt",
                    "text/plain");

                IngestionException exception = await Assert.ThrowsAsync<IngestionException>(() =>
                    reader.ReadAsync(definition, cancellationToken: TestContext.Current.CancellationToken));

                Assert.Equal(JobFailureCategoryType.Permanent, exception.Category);
                Assert.Equal("file_path_not_allowed", exception.Code);
            }
            finally
            {
                DeleteTemporaryDirectory(allowedRoot);
                DeleteTemporaryDirectory(blobRoot);
            }
        }

        [Fact]
        public async Task ReadWebPage_WithLoopbackAddress_ShouldFailBeforeConnect()
        {
            string allowedRoot = CreateTemporaryDirectory();
            string blobRoot = CreateTemporaryDirectory();
            try
            {
                SourceReader reader = CreateReader(allowedRoot, blobRoot);

                IngestionException exception = await Assert.ThrowsAsync<IngestionException>(() => reader.ReadAsync(
                    new WebPageSourceDefinition(new Uri("https://127.0.0.1/private")),
                    cancellationToken: TestContext.Current.CancellationToken));

                Assert.Equal("web_address_not_public", exception.Code);
            }
            finally
            {
                DeleteTemporaryDirectory(allowedRoot);
                DeleteTemporaryDirectory(blobRoot);
            }
        }

        [Fact]
        public async Task ReadFile_InsideConfiguredRoot_ShouldReturnContent()
        {
            string allowedRoot = CreateTemporaryDirectory();
            string blobRoot = CreateTemporaryDirectory();
            string path = Path.Join(allowedRoot, "source.txt");
            await File.WriteAllTextAsync(
                path,
                "safe content",
                Encoding.UTF8,
                TestContext.Current.CancellationToken);
            try
            {
                SourceReader reader = CreateReader(allowedRoot, blobRoot);
                SourceReadResult result = await reader.ReadAsync(
                    new FileSourceDefinition(path, null, "source.txt", "text/plain"),
                    cancellationToken: TestContext.Current.CancellationToken);
                await using (result.Content)
                using (StreamReader streamReader = new(result.Content))
                {
                    Assert.Equal(
                        "safe content",
                        await streamReader.ReadToEndAsync(
                            cancellationToken: TestContext.Current.CancellationToken));
                }
            }
            finally
            {
                DeleteTemporaryDirectory(allowedRoot);
                DeleteTemporaryDirectory(blobRoot);
            }
        }

        [Fact]
        public async Task ReadRepositoryFile_WithMatchingManifest_ShouldReturnContent()
        {
            string root = CreateTemporaryDirectory();
            string blobRoot = CreateTemporaryDirectory();
            byte[] content = Encoding.UTF8.GetBytes("tracked content");
            await File.WriteAllBytesAsync(Path.Join(root, "tracked.txt"), content,
                TestContext.Current.CancellationToken);
            try
            {
                RepositoryFileImportOptions file = new(root, "tracked.txt",
                    Convert.ToHexStringLower(SHA256.HashData(content)), "tracked.txt", "text/plain", content.Length);
                RepositorySourceDefinition definition = new(Guid.NewGuid().ToString("D"), null,
                    new RepositoryScanPolicy());

                SourceReadResult result = await CreateReader(root, blobRoot).ReadAsync(definition, file,
                    TestContext.Current.CancellationToken);

                await using (result.Content)
                using (StreamReader reader = new(result.Content))
                {
                    Assert.Equal("tracked content", await reader.ReadToEndAsync(
                        TestContext.Current.CancellationToken));
                }
            }
            finally
            {
                DeleteTemporaryDirectory(root);
                DeleteTemporaryDirectory(blobRoot);
            }
        }

        private static SourceReader CreateReader(string allowedRoot, string blobRoot)
        {
            return new SourceReader(
                new FileSystemBlobStoreService(blobRoot),
                new RejectingConnectorClient(),
                Options.Create(new IngestionOptions { AllowedFileRoots = [allowedRoot] }));
        }

        private static string CreateTemporaryDirectory()
        {
            string path = Path.Join(
                Path.GetTempPath(),
                "espada-reader-tests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static void DeleteTemporaryDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
    }
}