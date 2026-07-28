using Espada.Domain.Enums;

namespace Espada.Domain.ValueObjects.SourceDefinitions
{
    public sealed record FileSourceDefinition : SourceDefinition
    {
        public FileSourceDefinition(string? localPath, BlobSourceReference? blob, string fileName, string mediaType)
        {
            if (string.IsNullOrWhiteSpace(localPath) == blob is null)
            {
                throw new ArgumentException("Exactly one file location must be provided.");
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
            ArgumentException.ThrowIfNullOrWhiteSpace(mediaType);

            LocalPath = localPath;
            Blob = blob;
            FileName = fileName;
            MediaType = mediaType;
        }

        public string? LocalPath { get; init; }

        public BlobSourceReference? Blob { get; init; }

        public string FileName { get; init; }

        public string MediaType { get; init; }

        public override SourceType SourceType => SourceType.File;

        public override string CanonicalLocator => LocalPath ?? $"blob:{Blob!.BlobHash}";
    }
}