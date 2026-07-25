using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.TestData
{
    internal static class ArtifactRevisionFactory
    {
        public static ArtifactRevision Create(
            Artifact artifact,
            ArtifactRevisionId revisionId,
            string? content,
            DateTimeOffset createdAtUtc)
        {
            DomainResult<ArtifactContent> contentResult =
                ArtifactContent.Create(content);

            if (contentResult.IsFailure)
            {
                throw new InvalidOperationException(
                    "ArtifactRevisionFactory received invalid content: " +
                    $"{contentResult.Error.Code} — " +
                    $"{contentResult.Error.Description}");
            }

            DomainResult<ArtifactRevision> revisionResult =
                artifact.CreateRevision(
                    revisionId,
                    contentResult.Value,
                    createdAtUtc);

            if (revisionResult.IsFailure)
            {
                throw new InvalidOperationException(
                    "ArtifactRevisionFactory could not create revision: " +
                    $"{revisionResult.Error.Code} — " +
                    $"{revisionResult.Error.Description}");
            }

            artifact.DequeueDomainEvents();

            return revisionResult.Value;
        }
    }
}