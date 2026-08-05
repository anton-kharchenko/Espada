using Espada.Application.ApplicationErrors;
using Espada.Application.Constants;
using Espada.Application.Contracts.Repositories;
using Espada.Application.Models;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects.SourceDefinitions;
using Espada.Infrastructure.Ingestion;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Espada.Infrastructure.Repositories.Scanning
{
    internal sealed class GitRepositoryScanner : IRepositoryScanner
    {
        private static readonly HashSet<string> DocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".docx", ".pdf"
        };

        private static readonly HashSet<string> TextExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".c", ".config", ".cpp", ".cs", ".css", ".csv", ".fs", ".go", ".h", ".hpp", ".html",
            ".ini", ".java", ".js", ".json", ".jsx", ".kt", ".kts", ".md", ".mdx", ".ps1", ".py",
            ".rs", ".scss", ".sh", ".sql", ".toml", ".ts", ".tsx", ".txt", ".vb", ".xml", ".yaml", ".yml"
        };

        private static readonly HashSet<string> SecretFileNames = new(StringComparer.OrdinalIgnoreCase)
        {
            ".env", ".npmrc", ".pypirc", "credentials", "credentials.json", "id_dsa", "id_ed25519",
            "id_rsa", "secrets.json"
        };

        public async Task<DomainResult<RepositoryScanResult>> ScanAsync(IReadOnlyList<string> localAliases,
            RepositoryScanPolicy scanPolicy, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(localAliases);
            ArgumentNullException.ThrowIfNull(scanPolicy);
            string? root = localAliases
                .Select(Path.GetFullPath)
                .FirstOrDefault(Directory.Exists);
            if (root is null)
            {
                return DomainResult.Failure<RepositoryScanResult>(
                    ImportJobApplicationErrors.RepositoryRootUnavailable);
            }

            byte[]? output = await RunGitAsync(root, cancellationToken);
            if (output is null)
            {
                return DomainResult.Failure<RepositoryScanResult>(
                    ImportJobApplicationErrors.RepositoryRootUnavailable);
            }

            List<RepositoryFileRecord> files = [];
            foreach (string entry in Encoding.UTF8.GetString(output).Split('\0',
                         StringSplitOptions.RemoveEmptyEntries))
            {
                int tab = entry.IndexOf('\t');
                if (tab < 0)
                {
                    continue;
                }

                string mode = entry[..entry.IndexOf(' ')];
                string relativePath = entry[(tab + 1)..].Replace('\\', '/');
                if (mode == "160000" || IsSecretLike(relativePath) || !TryGetMediaType(relativePath, out string mediaType))
                {
                    continue;
                }

                string? path = RepositoryPathResolver.Resolve(root, relativePath);
                if (path is null || !File.Exists(path))
                {
                    continue;
                }

                FileInfo file = new(path);
                if (file.Length == 0 || file.Length > scanPolicy.MaximumFileSizeBytes)
                {
                    continue;
                }

                if (!DocumentExtensions.Contains(file.Extension) && await IsBinaryAsync(path, cancellationToken))
                {
                    continue;
                }

                await using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite,
                    81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
                string hash = Convert.ToHexStringLower(await SHA256.HashDataAsync(stream, cancellationToken));
                files.Add(new RepositoryFileRecord(relativePath, hash, file.Name, mediaType, file.Length));
            }

            return DomainResult.Success(new RepositoryScanResult(root,
                files.OrderBy(file => file.RelativePath, StringComparer.Ordinal).ToArray()));
        }

        private static async Task<byte[]?> RunGitAsync(string root, CancellationToken cancellationToken)
        {
            using Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            process.StartInfo.ArgumentList.Add("-C");
            process.StartInfo.ArgumentList.Add(root);
            process.StartInfo.ArgumentList.Add("ls-files");
            process.StartInfo.ArgumentList.Add("--stage");
            process.StartInfo.ArgumentList.Add("-z");
            if (!process.Start())
            {
                return null;
            }

            await using MemoryStream output = new();
            Task copy = process.StandardOutput.BaseStream.CopyToAsync(output, cancellationToken);
            Task<string> error = process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            await Task.WhenAll(copy, error);
            return process.ExitCode == 0 ? output.ToArray() : null;
        }

        private static bool IsSecretLike(string relativePath)
        {
            string fileName = Path.GetFileName(relativePath);
            string extension = Path.GetExtension(fileName);
            return SecretFileNames.Contains(fileName) ||
                   extension.Equals(".key", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".p12", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".pfx", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".pem", StringComparison.OrdinalIgnoreCase) ||
                   fileName.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
                   fileName.Contains("token", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryGetMediaType(string path, out string mediaType)
        {
            string extension = Path.GetExtension(path);
            if (TextExtensions.Contains(extension) ||
                (extension.Length == 0 && Path.GetFileName(path) is "LICENSE" or "README"))
            {
                mediaType = extension.Equals(".md", StringComparison.OrdinalIgnoreCase)
                    ? IngestionMediaTypeConstants.Markdown
                    : IngestionMediaTypeConstants.PlainText;
                return true;
            }

            mediaType = extension.ToLowerInvariant() switch
            {
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".pdf" => "application/pdf",
                _ => string.Empty
            };
            return mediaType.Length > 0;
        }

        private static async Task<bool> IsBinaryAsync(string path, CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[8192];
            await using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite,
                buffer.Length, FileOptions.Asynchronous | FileOptions.SequentialScan);
            int read = await stream.ReadAsync(buffer, cancellationToken);
            return buffer.AsSpan(0, read).Contains((byte)0);
        }
    }
}