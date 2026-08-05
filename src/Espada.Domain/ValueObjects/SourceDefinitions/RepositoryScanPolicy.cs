namespace Espada.Domain.ValueObjects.SourceDefinitions
{
    public sealed record RepositoryScanPolicy
    {
        public const long DefaultMaximumFileSizeBytes = 1_048_576;

        public RepositoryScanPolicy(long maximumFileSizeBytes = DefaultMaximumFileSizeBytes)
        {
            if (maximumFileSizeBytes <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumFileSizeBytes));
            }

            MaximumFileSizeBytes = maximumFileSizeBytes;
        }

        public bool TrackedFilesOnly => true;

        public long MaximumFileSizeBytes { get; init; }
    }
}