namespace Espada.Tests.Api.TestData
{
    internal static class TestValues
    {
        public const string SourceName = "Project README";
        public const string SourceLocator = "file:///workspace/README.md";
        public const string ImportFailureCode = "SOURCE_READ_FAILED";
        public const string ImportFailureReason = "The source could not be read.";
        public const string ArtifactTitle = "Architecture overview";
        public const string RenamedArtifactTitle = "Updated architecture overview";
        public const string ArtifactContent = "# Architecture";
        public const string ArtifactRevisionContent = "# Updated architecture";
        public const string ChunkingStrategyVersion = "fixed-size-v1";
        public const string ChunkContent = "Espada is a local-first context runtime.";
        public const string EmbeddingModelIdentifier = "test-embedding-model";
        public const string EmbeddingModelVersion = "1";
        public const string ApiKey = "espada-api-tests-key";
        public const string ApiKeyHeader = "X-Espada-Api-Key";
    }
}