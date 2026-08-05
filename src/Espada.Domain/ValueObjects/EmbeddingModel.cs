using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects
{
    public sealed class EmbeddingModel : ValueObject
    {
        public const int IdentifierMaxLength = 200;
        public const int VersionMaxLength = 100;

        private EmbeddingModel(string identifier, string version)
        {
            Identifier = identifier;
            Version = version;
        }

        public string Identifier { get; }

        public string Version { get; }

        public static DomainResult<EmbeddingModel> Create(string? identifier, string? version)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return DomainResult<EmbeddingModel>.Failure(ChunkEmbeddingErrors.ModelIdentifierEmpty);
            }

            if (string.IsNullOrWhiteSpace(version))
            {
                return DomainResult<EmbeddingModel>.Failure(ChunkEmbeddingErrors.ModelVersionEmpty);
            }

            string normalizedIdentifier = identifier.Trim();
            string normalizedVersion = version.Trim();

            if (normalizedIdentifier.Length > IdentifierMaxLength)
            {
                return DomainResult<EmbeddingModel>.Failure(ChunkEmbeddingErrors.ModelIdentifierTooLong);
            }

            return normalizedVersion.Length > VersionMaxLength
                ? DomainResult<EmbeddingModel>.Failure(ChunkEmbeddingErrors.ModelVersionTooLong)
                : DomainResult<EmbeddingModel>.Success(new EmbeddingModel(normalizedIdentifier, normalizedVersion));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Identifier;
            yield return Version;
        }

        public override string ToString()
        {
            return $"{Identifier}@{Version}";
        }
    }
}