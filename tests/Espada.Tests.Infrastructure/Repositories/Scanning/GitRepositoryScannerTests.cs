using Espada.Application.ApplicationErrors;
using Espada.Application.Models;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects.SourceDefinitions;
using Espada.Infrastructure.Repositories.Scanning;
using System.Diagnostics;
using System.Text;

namespace Espada.Tests.Infrastructure.Repositories.Scanning
{
    public sealed class GitRepositoryScannerTests
    {
        [Fact]
        public async Task ScanAsync_ShouldIncludeOnlySupportedSafeTrackedFiles()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                RunGit(root, "init");
                await File.WriteAllTextAsync(Path.Join(root, "README.md"), "# Tracked",
                    TestContext.Current.CancellationToken);
                await File.WriteAllTextAsync(Path.Join(root, ".env"), "TOKEN=secret",
                    TestContext.Current.CancellationToken);
                await File.WriteAllBytesAsync(Path.Join(root, "binary.txt"), [1, 0, 2],
                    TestContext.Current.CancellationToken);
                await File.WriteAllTextAsync(Path.Join(root, "oversized.txt"), new string('x', 128),
                    TestContext.Current.CancellationToken);
                await File.WriteAllTextAsync(Path.Join(root, "untracked.txt"), "not tracked",
                    TestContext.Current.CancellationToken);
                RunGit(root, "add", "-f", "README.md", ".env", "binary.txt", "oversized.txt");

                DomainResult<RepositoryScanResult> result = await new GitRepositoryScanner().ScanAsync(
                    [root], new RepositoryScanPolicy(64), TestContext.Current.CancellationToken);

                Assert.True(result.IsSuccess, result.Error.Description);
                RepositoryScanResult scan = result.Value;
                RepositoryFileRecord file = Assert.Single(scan.Files);
                Assert.Equal("README.md", file.RelativePath);
                Assert.Equal("text/markdown", file.MediaType);
            }
            finally
            {
                DeleteTemporaryDirectory(root);
            }
        }

        [Fact]
        public async Task ScanAsync_WithoutAccessibleAlias_ShouldReturnFailure()
        {
            DomainResult<RepositoryScanResult> result = await new GitRepositoryScanner().ScanAsync(
                [Path.Join(Path.GetTempPath(), Guid.NewGuid().ToString("N"))], new RepositoryScanPolicy(),
                TestContext.Current.CancellationToken);

            Assert.True(result.IsFailure);
            Assert.Equal(ImportJobApplicationErrors.RepositoryRootUnavailable, result.Error);
        }

        private static void DeleteTemporaryDirectory(string root)
        {
            foreach (string path in Directory.EnumerateFileSystemEntries(root, "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(path, FileAttributes.Normal);
            }

            Directory.Delete(root, true);
        }

        private static string CreateTemporaryDirectory()
        {
            string path = Path.Join(Path.GetTempPath(), $"espada-repository-scan-{Guid.NewGuid():N}");
            Directory.CreateDirectory(path);
            return path;
        }

        private static void RunGit(string root, params string[] arguments)
        {
            using Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            process.StartInfo.ArgumentList.Add("-C");
            process.StartInfo.ArgumentList.Add(root);
            foreach (string argument in arguments)
            {
                process.StartInfo.ArgumentList.Add(argument);
            }

            process.Start();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            Assert.True(process.ExitCode == 0, error);
        }
    }
}