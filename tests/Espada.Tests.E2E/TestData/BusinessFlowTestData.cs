namespace Espada.Tests.E2E.TestData;

internal static class BusinessFlowTestData
{
    internal static class Lifecycle
    {
        public const string WorkspaceName = "E2E workspace";
        public const string SourceName = "E2E source";
        public const string SourceLocator = "https://example.com/e2e-source";
        public const string InitialArtifactTitle = "E2E artifact";
        public const string InitialRevisionContent = "# First revision";
        public const string SecondRevisionContent = "# Second revision";
        public const string RenamedArtifactTitle = "Renamed E2E artifact";
        public const string ChunkingStrategyVersion = "recursive-v1";
        public const int FirstChunkNumber = 1;
        public const string FirstChunkContent = "First chunk";
        public const int FirstChunkSourceStart = 0;
        public const int FirstChunkSourceLength = 11;
        public const int SecondChunkNumber = 2;
        public const string SecondChunkContent = "Second chunk";
        public const int SecondChunkSourceStart = 12;
        public const int SecondChunkSourceLength = 12;
        public const int ExpectedChunkCount = 2;
        public const string EmbeddingModelIdentifier = "test-embedding-model";
        public const string EmbeddingModelVersion = "1";

        public static float[] CreateEmbeddingVector() => [0.25f, -0.5f, 1.25f];
    }

    internal static class InvalidTransitions
    {
        public const string FirstWorkspaceName = "First workspace";
        public const string SecondWorkspaceName = "Second workspace";
        public const string SourceName = "Owned source";
        public const string SourceLocator = "https://example.com/owned-source";
        public const string ArtifactTitle = "Archived artifact";
        public const string InitialRevisionContent = "Initial content";
        public const string RejectedRevisionContent = "Rejected revision";
    }
}