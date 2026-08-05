using Espada.Domain.Errors;

namespace Espada.Tests.Domain.ValueObjects
{
    public sealed class EmbeddingModelTests
    {
        [Fact]
        public void Create_WithValidValues_ShouldCreateModel()
        {
            // Act
            EmbeddingModel model = EmbeddingModel.Create("openai/text-embedding-3-small", "2026-01").ShouldSucceed();

            // Assert
            model.Identifier.Should().Be("openai/text-embedding-3-small");

            model.Version.Should().Be("2026-01");
        }

        [Fact]
        public void Create_ShouldTrimIdentifierAndVersion()
        {
            // Act
            EmbeddingModel model = EmbeddingModel.Create("  ollama/nomic-embed-text  ", "  latest  ").ShouldSucceed();

            // Assert
            model.Identifier.Should().Be("ollama/nomic-embed-text");

            model.Version.Should().Be("latest");
        }

        [Theory]
        [MemberData(nameof(StringTheoryData.NullOrWhiteSpaceValues), MemberType = typeof(StringTheoryData))]
        public void Create_WithEmptyIdentifier_ShouldReturnExpectedError(string? identifier)
        {
            // Act
            DomainResult<EmbeddingModel> result = EmbeddingModel.Create(identifier, "2026-01");

            // Assert
            result.ShouldFailWith(ChunkEmbeddingErrors.ModelIdentifierEmpty);
        }

        [Theory]
        [MemberData(nameof(StringTheoryData.NullOrWhiteSpaceValues), MemberType = typeof(StringTheoryData))]
        public void Create_WithEmptyVersion_ShouldReturnExpectedError(string? version)
        {
            // Act
            DomainResult<EmbeddingModel> result = EmbeddingModel.Create("openai/text-embedding-3-small", version);

            // Assert
            result.ShouldFailWith(ChunkEmbeddingErrors.ModelVersionEmpty);
        }

        [Fact]
        public void Create_WithMaximumIdentifierLength_ShouldSucceed()
        {
            // Arrange
            string identifier = new('a', EmbeddingModel.IdentifierMaxLength);

            // Act
            EmbeddingModel model = EmbeddingModel.Create(identifier, "v1").ShouldSucceed();

            // Assert
            model.Identifier.Should().HaveLength(EmbeddingModel.IdentifierMaxLength);
        }

        [Fact]
        public void Create_AboveMaximumIdentifierLength_ShouldReturnExpectedError()
        {
            // Arrange
            string identifier = new('a', EmbeddingModel.IdentifierMaxLength + 1);

            // Act
            DomainResult<EmbeddingModel> result = EmbeddingModel.Create(identifier, "v1");

            // Assert
            result.ShouldFailWith(ChunkEmbeddingErrors.ModelIdentifierTooLong);
        }

        [Fact]
        public void Create_WithMaximumVersionLength_ShouldSucceed()
        {
            // Arrange
            string version = new('a', EmbeddingModel.VersionMaxLength);

            // Act
            EmbeddingModel model = EmbeddingModel.Create("local/model", version).ShouldSucceed();

            // Assert
            model.Version.Should().HaveLength(EmbeddingModel.VersionMaxLength);
        }

        [Fact]
        public void Create_AboveMaximumVersionLength_ShouldReturnExpectedError()
        {
            // Arrange
            string version = new('a', EmbeddingModel.VersionMaxLength + 1);

            // Act
            DomainResult<EmbeddingModel> result = EmbeddingModel.Create("local/model", version);

            // Assert
            result.ShouldFailWith(ChunkEmbeddingErrors.ModelVersionTooLong);
        }

        [Fact]
        public void ModelsWithSameIdentifierAndVersion_ShouldBeEqual()
        {
            // Arrange
            EmbeddingModel first = EmbeddingModel.Create("openai/text-embedding-3-small", "2026-01").ShouldSucceed();

            EmbeddingModel second = EmbeddingModel.Create("openai/text-embedding-3-small", "2026-01").ShouldSucceed();

            // Assert
            first.Should().Be(second);

            first.GetHashCode().Should().Be(second.GetHashCode());
        }

        [Fact]
        public void ModelsWithDifferentVersions_ShouldNotBeEqual()
        {
            // Arrange
            EmbeddingModel first = EmbeddingModel.Create("local/model", "v1").ShouldSucceed();

            EmbeddingModel second = EmbeddingModel.Create("local/model", "v2").ShouldSucceed();

            // Assert
            first.Should().NotBe(second);
        }

        [Fact]
        public void ModelsWithDifferentIdentifiers_ShouldNotBeEqual()
        {
            // Arrange
            EmbeddingModel first = EmbeddingModel.Create("provider/model-a", "v1").ShouldSucceed();

            EmbeddingModel second = EmbeddingModel.Create("provider/model-b", "v1").ShouldSucceed();

            // Assert
            first.Should().NotBe(second);
        }

        [Fact]
        public void ModelComparison_ShouldRemainCaseSensitive()
        {
            // Arrange
            EmbeddingModel first = EmbeddingModel.Create("provider/model", "v1").ShouldSucceed();

            EmbeddingModel second = EmbeddingModel.Create("Provider/Model", "v1").ShouldSucceed();

            // Assert
            first.Should().NotBe(second);
        }

        [Fact]
        public void ToString_ShouldContainIdentifierAndVersion()
        {
            // Arrange
            EmbeddingModel model = EmbeddingModel.Create("ollama/nomic-embed-text", "v2").ShouldSucceed();

            // Act
            string value = model.ToString();

            // Assert
            value.Should().Be("ollama/nomic-embed-text@v2");
        }
    }
}