using Espada.Domain.Enums;

namespace Espada.Domain.ValueObjects.SourceDefinitions
{
    public sealed record RepositorySourceDefinition : SourceDefinition
    {
        public const int RepositoryIdentityMaxLength = 512;

        public RepositorySourceDefinition(
            string repositoryIdentity,
            string? canonicalRemoteUri,
            RepositoryScanPolicy scanPolicy)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(repositoryIdentity);
            ArgumentNullException.ThrowIfNull(scanPolicy);

            string normalizedIdentity = repositoryIdentity.Trim();
            if (normalizedIdentity.Length > RepositoryIdentityMaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(repositoryIdentity));
            }

            string? normalizedRemoteUri = string.IsNullOrWhiteSpace(canonicalRemoteUri)
                ? null
                : canonicalRemoteUri.Trim();
            if (normalizedRemoteUri?.Length > SourceLocator.MaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(canonicalRemoteUri));
            }

            RepositoryIdentity = normalizedIdentity;
            CanonicalRemoteUri = normalizedRemoteUri;
            ScanPolicy = scanPolicy;
        }

        public string RepositoryIdentity { get; init; }

        public string? CanonicalRemoteUri { get; init; }

        public RepositoryScanPolicy ScanPolicy { get; init; }

        public override SourceType SourceType => SourceType.Repository;

        public override string CanonicalLocator => $"repository:{RepositoryIdentity}";
    }
}