namespace Espada.Application.Contracts.Ingestion;

public static class IngestionMediaTypes
{
    public const string Binary = "application/octet-stream";
    public const string Json = "application/json";
    public const string Pdf = "application/pdf";
    public const string Html = "text/html";
    public const string Xhtml = "application/xhtml+xml";
    public const string PlainText = "text/plain";
    public const string Utf8PlainText = "text/plain; charset=utf-8";
    public const string Markdown = "text/markdown";
    public const string OpenXmlDocument = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
    public const string OpenXmlSpreadsheet = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public const string OpenXmlPresentation = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
}

public static class IngestionFileExtensions
{
    public const string LegacyDocument = ".doc";
    public const string LegacySpreadsheet = ".xls";
    public const string LegacyPresentation = ".ppt";
    public const string Pdf = ".pdf";
    public const string OpenXmlDocument = ".docx";
    public const string OpenXmlSpreadsheet = ".xlsx";
    public const string OpenXmlPresentation = ".pptx";
    public const string Html = ".html";
    public const string ShortHtml = ".htm";
    public const string Json = ".json";
    public const string Markdown = ".md";
    public const string LongMarkdown = ".markdown";
    public const string Text = ".txt";
}

public static class IngestionFailureCodes
{
    public const string ImportNotFound = "import_not_found";
    public const string SourceNotFound = "source_not_found";
    public const string StageMismatch = "stage_mismatch";
    public const string UnknownStage = "unknown_stage";
    public const string UnsupportedChunkingStrategy = "unsupported_chunking_strategy";
    public const string EmptyChunkBatch = "empty_chunk_batch";
    public const string EmptyExtractedText = "empty_extracted_text";
    public const string InvalidImportOptions = "invalid_import_options";
    public const string InvalidEmbeddingModel = "invalid_embedding_model";
    public const string InvalidEmbeddingVector = "invalid_embedding_vector";
    public const string EmbeddingCountMismatch = "embedding_count_mismatch";
    public const string EmbeddingDimensionMismatch = "embedding_dimension_mismatch";
    public const string MissingArtifactReference = "missing_artifact_reference";
    public const string MissingRevisionReference = "missing_revision_reference";
    public const string UnsupportedFormat = "unsupported_format";
    public const string MalformedSource = "malformed_source";
    public const string ExtractedSizeLimitExceeded = "extracted_size_limit_exceeded";
    public const string ParseTimeout = "parse_timeout";
    public const string FileNotFound = "file_not_found";
    public const string FilePathNotAllowed = "file_path_not_allowed";
    public const string FileReparsePointRejected = "file_reparse_point_rejected";
    public const string InsecureRedirect = "insecure_redirect";
    public const string InvalidRedirect = "invalid_redirect";
    public const string LegacySourceUnsupported = "legacy_source_unsupported";
    public const string RawSizeLimitExceeded = "raw_size_limit_exceeded";
    public const string ReadTimeout = "read_timeout";
    public const string RedirectLimitExceeded = "redirect_limit_exceeded";
    public const string SourceUnavailable = "source_unavailable";
    public const string UnknownSourceDefinition = "unknown_source_definition";
    public const string WebAddressNotPublic = "web_address_not_public";
    public const string WebSourceRejected = "web_source_rejected";
    public const string WebSourceUnavailable = "web_source_unavailable";
}

public static class ImportPipelineDiscriminators
{
    public const string Artifact = "artifact";
    public const string Revision = "revision";
    public const string ChunkBatch = "chunk-batch";
    public const string RawBlob = "raw";
    public const string ParsedBlob = "parsed";
    public const string ChunkingVersion = "v1";
}