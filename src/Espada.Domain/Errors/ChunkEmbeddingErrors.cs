using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Errors
{
    public static class ChunkEmbeddingErrors
    {
        public static readonly DomainError ModelIdentifierEmpty = new("ChunkEmbedding.Model.Identifier.Empty", "Embedding model identifier cannot be empty.");

        public static readonly DomainError ModelIdentifierTooLong = new("ChunkEmbedding.Model.Identifier.TooLong", $"Embedding model identifier cannot exceed {EmbeddingModel.IdentifierMaxLength} characters.");

        public static readonly DomainError ModelVersionEmpty = new("ChunkEmbedding.Model.Version.Empty", "Embedding model version cannot be empty.");

        public static readonly DomainError ModelVersionTooLong = new("ChunkEmbedding.Model.Version.TooLong", $"Embedding model version cannot exceed {EmbeddingModel.VersionMaxLength} characters.");

        public static readonly DomainError DimensionsInvalid = new("ChunkEmbedding.Dimensions.Invalid", "Embedding dimensions must be greater than zero.");
    }
}