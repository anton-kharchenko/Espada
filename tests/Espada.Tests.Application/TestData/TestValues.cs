namespace Espada.Tests.Application.TestData
{
    public class TestValues
    {
        public const string WorkspaceName = "Espada workspace";

        public const string AnotherWorkspaceName = "Another workspace";
        
        public const string SourceName = "Project README";

        public const string SourceLocator = "file:///workspace/README.md";

        public const string AnotherSourceLocator = "https://example.com/document";
        
        public const string ImportFailureCode = "SOURCE_READ_FAILED";

        public const string ImportFailureReason = "The source could not be read.";

        public const string AnotherImportFailureCode = "PARSER_FAILED";

        public const string AnotherImportFailureReason = "The source content could not be parsed.";
        
        public const string ArtifactTitle = "Project README";

        public const string ArtifactContent = """
                                              # Espada

                                              Local-first context runtime.
                                              """;

        public const string AnotherArtifactContent = """
                                                     # Espada

                                                     Updated artifact content.
                                                     """;
    }
}