namespace Espada.Domain.ValueObjects.SourceDefinitions
{
    public sealed record BlobSourceReference
    {
        public BlobSourceReference(string blobHash, string fileName, string mediaType)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(blobHash);
            ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
            ArgumentException.ThrowIfNullOrWhiteSpace(mediaType);

            BlobHash = blobHash;
            FileName = fileName;
            MediaType = mediaType;
        }

        public string BlobHash { get; init; }

        public string FileName { get; init; }

        public string MediaType { get; init; }
    }
}