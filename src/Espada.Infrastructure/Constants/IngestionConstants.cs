namespace Espada.Infrastructure.Constants;

public static class IngestionConstants
{
    public const string SectionName = "Ingestion";

    public const long DefaultMaximumRawBytes = 25L * 1024 * 1024;

    public const int DefaultMaximumExtractedBytes = 10 * 1024 * 1024;
}