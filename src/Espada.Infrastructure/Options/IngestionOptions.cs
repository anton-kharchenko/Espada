using Espada.Infrastructure.Constants;

namespace Espada.Infrastructure.Options
{
    public sealed class IngestionOptions
    {
        public List<string> AllowedFileRoots { get; set; } = [];

        public string BlobRoot { get; set; } = string.Empty;

        public long MaximumRawBytes { get; set; } = IngestionConstants.DefaultMaximumRawBytes;

        public int MaximumExtractedBytes { get; set; } = IngestionConstants.DefaultMaximumExtractedBytes;

        public int OperationTimeoutSeconds { get; set; } = 60;

        public bool IsValid()
        {
            return MaximumRawBytes > 0 && MaximumExtractedBytes > 0 && OperationTimeoutSeconds > 0;
        }
    }
}