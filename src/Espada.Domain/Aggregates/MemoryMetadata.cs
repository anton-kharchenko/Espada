using Espada.Domain.Enums;
using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Aggregates
{
    public sealed class MemoryMetadata : AggregateRoot<MemoryId>
    {
        public const int IdentityMaxLength = 200;

        private MemoryMetadata()
        {
        }

        private MemoryMetadata(MemoryId id, ArtifactId artifactId, ArtifactRevisionId artifactRevisionId,
            ArtifactKindType kindType, MemoryCategoryType categoryType, decimal confidence, bool userConfirmed,
            string clientIdentity, string? sessionIdentity, DateTimeOffset capturedAtUtc,
            MemoryId? supersededMemoryId) : base(id)
        {
            ArtifactId = artifactId;
            ArtifactRevisionId = artifactRevisionId;
            KindType = kindType;
            CategoryType = categoryType;
            Confidence = confidence;
            UserConfirmed = userConfirmed;
            ClientIdentity = clientIdentity;
            SessionIdentity = sessionIdentity;
            CapturedAtUtc = capturedAtUtc;
            SupersededMemoryId = supersededMemoryId;
        }

        public ArtifactId ArtifactId { get; private set; } = null!;
        public ArtifactRevisionId ArtifactRevisionId { get; private set; } = null!;
        public ArtifactKindType KindType { get; private set; } = null!;
        public MemoryCategoryType CategoryType { get; private set; } = null!;
        public decimal Confidence { get; private set; }
        public bool UserConfirmed { get; private set; }
        public string ClientIdentity { get; private set; } = string.Empty;
        public string? SessionIdentity { get; private set; }
        public DateTimeOffset CapturedAtUtc { get; private set; }
        public MemoryId? SupersededMemoryId { get; private set; }

        internal static DomainResult<MemoryMetadata> Create(MemoryId id, ArtifactRevision revision,
            MemoryCategoryType categoryType, decimal confidence, bool userConfirmed, string? clientIdentity,
            string? sessionIdentity, DateTimeOffset capturedAtUtc, MemoryId? supersededMemoryId)
        {
            ArgumentNullException.ThrowIfNull(id);
            ArgumentNullException.ThrowIfNull(revision);
            ArgumentNullException.ThrowIfNull(categoryType);
            if (confidence is < 0 or > 1)
            {
                return DomainResult<MemoryMetadata>.Failure(MemoryErrors.ConfidenceOutOfRange);
            }

            if (string.IsNullOrWhiteSpace(clientIdentity))
            {
                return DomainResult<MemoryMetadata>.Failure(MemoryErrors.ClientIdentityEmpty);
            }

            if (clientIdentity.Trim().Length > IdentityMaxLength || sessionIdentity?.Trim().Length > IdentityMaxLength)
            {
                return DomainResult<MemoryMetadata>.Failure(MemoryErrors.IdentityTooLong);
            }

            if (supersededMemoryId?.Equals(id) == true)
            {
                return DomainResult<MemoryMetadata>.Failure(MemoryErrors.SupersedesSelf);
            }

            return DomainResult<MemoryMetadata>.Success(new MemoryMetadata(id, revision.ArtifactId, revision.Id,
                ArtifactKindType.Memory, categoryType, confidence, userConfirmed, clientIdentity.Trim(),
                Normalize(sessionIdentity), capturedAtUtc, supersededMemoryId));
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}