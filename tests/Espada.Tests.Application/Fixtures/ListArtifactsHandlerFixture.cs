using AutoMapper;
using Espada.Application.Mappings;
using Espada.Application.UseCases.Artifacts.Queries.ListArtifacts;
using Espada.Domain.Aggregates;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;
using Microsoft.Extensions.Logging.Abstractions;

namespace Espada.Tests.Application.Fixtures
{
    internal sealed class ListArtifactsHandlerFixture
    {
        private readonly IMapper _mapper = new MapperConfiguration(
            options => options.AddProfile<ApplicationMappingProfile>(),
            NullLoggerFactory.Instance).CreateMapper();

        public WorkspaceRepositorySpy WorkspaceRepository { get; } = new();

        public ArtifactRepositorySpy ArtifactRepository { get; } = new();

        public ListArtifactsQueryHandler CreateHandler()
        {
            return new ListArtifactsQueryHandler(
                WorkspaceRepository,
                ArtifactRepository,
                _mapper);
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
                .WithId(TestIds.DefaultArtifactId)
                .WithTitle(TestValues.ArtifactTitle)
                .CreatedAt(TestDates.ArtifactCreatedAtUtc)
                .BuildWithFirstRevisionWithoutPendingEvents();

            Artifact second = new ArtifactBuilder()
                .WithId(TestIds.AnotherArtifactId)
                .WithTitle(TestValues.AnotherArtifactTitle)
                .CreatedAt(TestDates.ArtifactSecondRevisionCreatedAtUtc)
                .BuildWithFirstRevisionWithoutPendingEvents(
                    TestDates.ArtifactSecondRevisionCreatedAtUtc);

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