using Espada.Application.UseCases.Artifacts.Queries.ListArtifacts;
using Espada.Domain.Aggregates;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.Fixtures
{
    internal sealed class ListArtifactsHandlerFixture
    {
        public WorkspaceRepositorySpy WorkspaceRepository { get; } = new();

        public ArtifactRepositorySpy ArtifactRepository { get; } = new();

        public ListArtifactsQueryHandler CreateHandler()
        {
            return new ListArtifactsQueryHandler(
                WorkspaceRepository,
                ArtifactRepository);
        }

        public Workspace GivenWorkspaceExists()
        {
            Workspace workspace = new WorkspaceBuilder()
                .BuildWithoutPendingEvents();

            WorkspaceRepository.WorkspaceToReturn = workspace;

            return workspace;
        }

        public void GivenWorkspaceDoesNotExist()
        {
            WorkspaceRepository.WorkspaceToReturn = null;
        }

        public (Artifact First, Artifact Second) GivenArtifactsExist()
        {
            Artifact first = new ArtifactBuilder()
                .WithId(ArtifactTestIds.DefaultArtifactId)
                .WithTitle(ArtifactTestValues.Title)
                .CreatedAt(ArtifactTestDates.CreatedAtUtc)
                .BuildWithFirstRevisionWithoutPendingEvents();

            Artifact second = new ArtifactBuilder()
                .WithId(ArtifactTestIds.AnotherArtifactId)
                .WithTitle(ArtifactTestValues.AnotherTitle)
                .CreatedAt(ArtifactTestDates.SecondRevisionCreatedAtUtc)
                .BuildWithFirstRevisionWithoutPendingEvents(
                    ArtifactTestDates.SecondRevisionCreatedAtUtc);

            ArtifactRepository.ArtifactsToReturn = [first, second];

            return (first, second);
        }

        public void GivenNoArtifactsExist()
        {
            ArtifactRepository.ArtifactsToReturn =
                Array.Empty<Artifact>();
        }
    }
}