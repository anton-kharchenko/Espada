using Espada.Application.Contracts.Ingestion;
using Espada.Application.Contracts.Jobs;
using Espada.Application.Enums;
using Espada.Application.Exceptions;
using Espada.Application.Models;
using Espada.Domain.ValueObjects;
using Espada.Domain.ValueObjects.SourceDefinitions;
using Espada.Infrastructure.Ingestion;
using Espada.Infrastructure.Options;
using Espada.Infrastructure.Services;
using Espada.Tests.Infrastructure.Ingestion.Fakes;
using Microsoft.Extensions.Options;
using System.Text;

namespace Espada.Tests.Infrastructure.Ingestion;

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

            IngestionException exception = await Assert.ThrowsAsync<IngestionException>(
                () => reader.ReadAsync(definition, TestContext.Current.CancellationToken));

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

            IngestionException exception = await Assert.ThrowsAsync<IngestionException>(
                () => reader.ReadAsync(
                    new WebPageSourceDefinition(new Uri("https://127.0.0.1/private")),
                    TestContext.Current.CancellationToken));

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
                TestContext.Current.CancellationToken);
            await using (result.Content)
            using (StreamReader streamReader = new(result.Content))
            {
                Assert.Equal(
                    "safe content",
                    await streamReader.ReadToEndAsync(
                        TestContext.Current.CancellationToken));
            }
        }
        finally
        {
            DeleteTemporaryDirectory(allowedRoot);
            DeleteTemporaryDirectory(blobRoot);
        }
    }

    private static SourceReader CreateReader(string allowedRoot, string blobRoot) =>
        new(
            new FileSystemBlobStoreService(blobRoot),
            new RejectingConnectorClient(),
            Options.Create(new IngestionOptions { AllowedFileRoots = [allowedRoot] }));

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
            Directory.Delete(path, recursive: true);
        }
    }

}