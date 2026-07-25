using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.TestData.Builder
{
    internal sealed class ArtifactBuilder
    {
        private ArtifactId _id =
            ArtifactTestIds.DefaultArtifactId;

        private WorkspaceId _workspaceId =
            TestIds.DefaultWorkspaceId;

        private string? _title =
            ArtifactTestValues.Title;

        private ArtifactType _type =
            ArtifactType.Markdown;

        private DateTimeOffset _createdAtUtc =
            ArtifactTestDates.CreatedAtUtc;

        public ArtifactBuilder WithId(ArtifactId id)
        {
            _id = id;
            return this;
        }

        public ArtifactBuilder InWorkspace(WorkspaceId workspaceId)
        {
            _workspaceId = workspaceId;
            return this;
        }

        public ArtifactBuilder WithTitle(string? title)
        {
            _title = title;
            return this;
        }

        public ArtifactBuilder WithType(ArtifactType type)
        {
            _type = type;
            return this;
        }

        public ArtifactBuilder CreatedAt(DateTimeOffset createdAtUtc)
        {
            _createdAtUtc = createdAtUtc;
            return this;
        }

        public DomainResult<Artifact> BuildResult()
        {
            DomainResult<ArtifactTitle> titleResult =
                ArtifactTitle.Create(_title);

            if (titleResult.IsFailure)
            {
                return DomainResult<Artifact>.Failure(
                    titleResult.Error);
            }

            return Artifact.Create(
                _id,
                _workspaceId,
                titleResult.Value,
                _type,
                _createdAtUtc);
        }

        public Artifact Build()
        {
            DomainResult<Artifact> result = BuildResult();

            if (result.IsFailure)
            {
                throw new InvalidOperationException(
                    "ArtifactBuilder produced an invalid artifact: " +
                    $"{result.Error.Code} вЂ” {result.Error.Description}");
            }

            return result.Value;
        }

        public Artifact BuildWithoutPendingEvents()
        {
            Artifact artifact = Build();

            artifact.DequeueDomainEvents();

            return artifact;
        }

        public Artifact BuildWithFirstRevisionWithoutPendingEvents(
            DateTimeOffset? revisionCreatedAtUtc = null)
        {
            Artifact artifact = BuildWithoutPendingEvents();

            DomainResult<ArtifactContent> contentResult =
                ArtifactContent.Create(
                    ArtifactTestValues.FirstContent);

            if (contentResult.IsFailure)
            {
                throw new InvalidOperationException(
                    "ArtifactBuilder received invalid content.");
            }

            DomainResult<ArtifactRevision> revisionResult =
                artifact.CreateRevision(
                    ArtifactTestIds.FirstRevisionId,
                    contentResult.Value,
                    revisionCreatedAtUtc ??
                    ArtifactTestDates.FirstRevisionCreatedAtUtc);

            if (revisionResult.IsFailure)
            {
                throw new InvalidOperationException(
                    "ArtifactBuilder could not create revision: " +
                    $"{revisionResult.Error.Code} вЂ” " +
                    $"{revisionResult.Error.Description}");
            }

            artifact.DequeueDomainEvents();

            return artifact;
        }

        public Artifact BuildArchivedWithoutPendingEvents()
        {
            Artifact artifact =
                BuildWithFirstRevisionWithoutPendingEvents();

            DomainResult archiveResult =
                artifact.Archive(
                    ArtifactTestDates.ArchivedAtUtc);

            if (archiveResult.IsFailure)
            {
                throw new InvalidOperationException(
                    "ArtifactBuilder could not archive artifact: " +
                    $"{archiveResult.Error.Code} вЂ” " +
                    $"{archiveResult.Error.Description}");
            }

            artifact.DequeueDomainEvents();

            return artifact;
        }
    }
}