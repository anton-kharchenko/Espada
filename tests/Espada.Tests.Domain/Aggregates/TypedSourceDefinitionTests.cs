using Espada.Domain.ValueObjects.SourceDefinitions;

namespace Espada.Tests.Domain.Aggregates
{
    public sealed class TypedSourceDefinitionTests
    {
        [Fact]
        public void Create_WithWebPageDefinition_ShouldKeepTypedDefinition()
        {
            WebPageSourceDefinition definition = new(new Uri("https://example.com/docs"));

            Source source = Source.Create(
                TestIds.DefaultSourceId,
                TestIds.DefaultWorkspaceId,
                SourceName.Create("Product documentation").ShouldSucceed(),
                definition,
                TestDates.SourceCreatedAtUtc).ShouldSucceed();

            source.Definition.Should().Be(definition);
            source.Type.Should().Be(SourceType.WebPage);
            source.Locator.Value.Should().Be("https://example.com/docs");
        }

        [Fact]
        public void FileDefinition_ShouldRequireExactlyOneLocation()
        {
            Action noLocation = () => new FileSourceDefinition(null, null, "readme.md", "text/markdown");
            Action twoLocations = () => new FileSourceDefinition(
                "C:\\workspace\\README.md",
                new BlobSourceReference("sha256:abc", "readme.md", "text/markdown"),
                "readme.md",
                "text/markdown");

            noLocation.Should().Throw<ArgumentException>();
            twoLocations.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ConversationDefinition_ShouldRejectMoreThanConfiguredHardLimit()
        {
            ConversationMessage[] messages = Enumerable.Range(0, 5001)
                .Select(index => new ConversationMessage("user", null, $"message-{index}", null))
                .ToArray();

            Action create = () => new ConversationSourceDefinition("Long conversation", messages);

            create.Should().Throw<ArgumentOutOfRangeException>();
        }
        [Fact]
        public void RepositoryDefinition_ShouldKeepIdentityAndTrackedOnlyPolicy()
        {
            RepositorySourceDefinition definition = new(
                "11111111-1111-1111-1111-111111111111",
                null,
                new RepositoryScanPolicy());

            Source source = Source.Create(
                TestIds.DefaultSourceId,
                TestIds.DefaultWorkspaceId,
                SourceName.Create("Local repository").ShouldSucceed(),
                definition,
                TestDates.SourceCreatedAtUtc).ShouldSucceed();

            source.Type.Should().Be(SourceType.Repository);
            source.Locator.Value.Should().Be("repository:11111111-1111-1111-1111-111111111111");
            definition.ScanPolicy.TrackedFilesOnly.Should().BeTrue();
        }

    }
}