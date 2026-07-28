using Espada.Application.Contracts.Blobs;
using Espada.Application.Models;
using System.Security.Cryptography;

namespace Espada.Infrastructure.Services
{
    internal sealed class FileSystemBlobStoreService : IBlobStoreService
    {
        private readonly string _root;
        private readonly string _temporaryRoot;

        public FileSystemBlobStoreService(string root)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(root);
            _root = Path.GetFullPath(root);
            _temporaryRoot = Path.Join(_root, ".tmp");
            Directory.CreateDirectory(_temporaryRoot);
        }

        public async Task<BlobDescriptor> PutAsync(Stream content, BlobWriteOptions options,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(content);
            ArgumentNullException.ThrowIfNull(options);
            ArgumentException.ThrowIfNullOrWhiteSpace(options.MediaType);

            string temporaryPath = Path.Join(_temporaryRoot, Guid.NewGuid().ToString("N"));
            long length = 0;
            using IncrementalHash hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            try
            {
                await using (FileStream destination = new(temporaryPath, FileMode.CreateNew, FileAccess.Write,
                                 FileShare.None, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan))
                {
                    byte[] buffer = new byte[81920];
                    int read;
                    while ((read = await content.ReadAsync(buffer, cancellationToken)) > 0)
                    {
                        hasher.AppendData(buffer, 0, read);
                        await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                        length += read;
                    }

                    await destination.FlushAsync(cancellationToken);
                }

                BlobHash hash = new(Convert.ToHexStringLower(hasher.GetHashAndReset()));
                string destinationPath = ResolvePath(hash);
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                try
                {
                    File.Move(temporaryPath, destinationPath, false);
                }
                catch (IOException) when (File.Exists(destinationPath))
                {
                    File.Delete(temporaryPath);
                }

                return new BlobDescriptor(hash, length, options.MediaType);
            }
            catch
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }

                throw;
            }
        }

        public Task<Stream> OpenReadAsync(BlobHash hash, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Stream stream = new FileStream(ResolvePath(hash), FileMode.Open, FileAccess.Read, FileShare.Read, 81920,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            return Task.FromResult(stream);
        }

        public Task DeleteAsync(BlobHash hash, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string path = ResolvePath(hash);
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(BlobHash hash, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(File.Exists(ResolvePath(hash)));
        }

        private string ResolvePath(BlobHash hash)
        {
            return Path.Join(_root, hash.Value[..2], hash.Value[2..4], hash.Value);
        }
    }
}