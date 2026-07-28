using AutoMapper;
using Espada.Application.Mappings;
using Espada.Application.UseCases.Bindings.Commands.SetBinding;
using Espada.Application.UseCases.Bindings.Common;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData;
using Microsoft.Extensions.Logging.Abstractions;

namespace Espada.Tests.Application.UseCases.Bindings.Commands.SetBinding
{
    public sealed class SetBindingCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldPersistSelectorsFromSameWorkspace()
        {
            WorkspaceId workspaceId = TestIds.DefaultWorkspaceId;
            Workspace workspace = Workspace.Create(
                workspaceId,
                WorkspaceName.Create(TestValues.WorkspaceName).Value,
                WorkspaceType.Personal,
                null,
                TestDates.ArtifactCreatedAtUtc).Value;
            Project project = Project.Create(
                ProjectId.Create(Guid.NewGuid()),
                workspaceId,
                "Espada",
                "https://github.com/example/espada",
                [],
                TestDates.ArtifactCreatedAtUtc).Value;
            ProjectTask task = project.CreateTask(
                TaskId.Create(Guid.NewGuid()),
                "Implement bindings",
                TestDates.ArtifactCreatedAtUtc).Value;
            Artifact artifact = Artifact.Create(
                TestIds.DefaultArtifactId,
                workspaceId,
                ArtifactTitle.Create(TestValues.ArtifactTitle).Value,
                ArtifactKindType.Instruction,
                ArtifactType.Markdown,
                TestDates.ArtifactCreatedAtUtc).Value;
            ArtifactRevision revision = artifact.CreateRevision(
                TestIds.DefaultArtifactRevisionId,
                ArtifactContent.Create(TestValues.ArtifactContent).Value,
                TestDates.ArtifactCreatedAtUtc).Value;
            ArtifactRepositorySpy artifactRepository = new() { ArtifactToReturn = artifact };
            ArtifactRevisionRepositorySpy revisionRepository = new() { ArtifactRevisionToReturn = revision };
            ProjectRepositorySpy projectRepository = new() { ProjectToReturn = project };
            ProjectTaskRepositorySpy taskRepository = new() { TaskToReturn = task };
            BindingRepositorySpy bindingRepository = new();
            UnitOfWorkSpy unitOfWork = new();
            IMapper mapper = new MapperConfiguration(
                options => options.AddProfile<ApplicationMappingProfile>(),
                NullLoggerFactory.Instance).CreateMapper();
            SetBindingCommandHandler handler = new(
                new WorkspaceRepositorySpy { WorkspaceToReturn = workspace },
                artifactRepository,
                revisionRepository,
                new OrganizationRepositorySpy(),
                projectRepository,
                taskRepository,
                bindingRepository,
                unitOfWork,
                new TestClockService(TestDates.ArtifactCreatedAtUtc),
                mapper);
            SetBindingCommand command = new(
                workspaceId.Value,
                artifact.Id.Value,
                ProjectId: project.Id.Value,
                RepositoryCanonicalUri: project.CanonicalRemoteUri,
                RepositoryRelativePathPrefix: "src/Espada.Application",
                Branch: "feature/context",
                TaskId: task.Id.Value,
                Agent: "codex");

            DomainResult<BindingResponse> result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

            BindingResponse response = result.ShouldSucceed();
            response.ProjectId.Should().Be(project.Id.Value);
            response.TaskId.Should().Be(task.Id.Value);
            response.RepositoryRelativePathPrefix.Should().Be("src/Espada.Application");
            response.Branch.Should().Be("feature/context");
            response.Agent.Should().Be("codex");
            bindingRepository.UpsertedBinding.Should().NotBeNull();
            unitOfWork.SaveChangesCallCount.Should().Be(1);
        }

        [Fact]
        public async Task Handle_WithBindingFromAnotherWorkspace_ShouldRejectOverwrite()
        {
            WorkspaceId workspaceId = TestIds.DefaultWorkspaceId;
            WorkspaceId otherWorkspaceId = WorkspaceId.Create(Guid.NewGuid());
            Workspace workspace = Workspace.Create(
                workspaceId,
                WorkspaceName.Create(TestValues.WorkspaceName).Value,
                WorkspaceType.Personal,
                null,
                TestDates.ArtifactCreatedAtUtc).Value;
            Workspace otherWorkspace = Workspace.Create(
                otherWorkspaceId,
                WorkspaceName.Create("Other workspace").Value,
                WorkspaceType.Personal,
                null,
                TestDates.ArtifactCreatedAtUtc).Value;
            BindingId bindingId = BindingId.Create(Guid.NewGuid());
            Artifact artifact = Artifact.Create(
                TestIds.DefaultArtifactId,
                workspaceId,
                ArtifactTitle.Create(TestValues.ArtifactTitle).Value,
                ArtifactKindType.Instruction,
                ArtifactType.Markdown,
                TestDates.ArtifactCreatedAtUtc).Value;
            ArtifactRevision revision = artifact.CreateRevision(
                TestIds.DefaultArtifactRevisionId,
                ArtifactContent.Create(TestValues.ArtifactContent).Value,
                TestDates.ArtifactCreatedAtUtc).Value;
            Artifact otherArtifact = Artifact.Create(
                ArtifactId.Create(Guid.NewGuid()),
                otherWorkspaceId,
                ArtifactTitle.Create("Other artifact").Value,
                ArtifactKindType.Instruction,
                ArtifactType.Markdown,
                TestDates.ArtifactCreatedAtUtc).Value;
            ArtifactRevision otherRevision = otherArtifact.CreateRevision(
                ArtifactRevisionId.Create(Guid.NewGuid()),
                ArtifactContent.Create("Other content").Value,
                TestDates.ArtifactCreatedAtUtc).Value;
            Binding existingBinding = otherArtifact.CreateBinding(
                bindingId,
                otherRevision,
                otherWorkspace,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                TestDates.ArtifactCreatedAtUtc).Value;
            BindingRepositorySpy bindingRepository = new() { BindingToReturn = existingBinding };
            UnitOfWorkSpy unitOfWork = new();
            IMapper mapper = new MapperConfiguration(
                options => options.AddProfile<ApplicationMappingProfile>(),
                NullLoggerFactory.Instance).CreateMapper();
            SetBindingCommandHandler handler = new(
                new WorkspaceRepositorySpy { WorkspaceToReturn = workspace },
                new ArtifactRepositorySpy { ArtifactToReturn = artifact },
                new ArtifactRevisionRepositorySpy { ArtifactRevisionToReturn = revision },
                new OrganizationRepositorySpy(),
                new ProjectRepositorySpy(),
                new ProjectTaskRepositorySpy(),
                bindingRepository,
                unitOfWork,
                new TestClockService(TestDates.ArtifactCreatedAtUtc),
                mapper);
            SetBindingCommand command = new(
                workspaceId.Value,
                artifact.Id.Value,
                bindingId.Value);

            DomainResult<BindingResponse> result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Binding.NotFoundInWorkspace");
            bindingRepository.UpsertedBinding.Should().BeNull();
            unitOfWork.SaveChangesCallCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_WithOrganizationFromAnotherWorkspace_ShouldRejectBinding()
        {
            DateTimeOffset createdAtUtc = TestDates.ArtifactCreatedAtUtc;
            Organization workspaceOrganization = Organization.Create(
                OrganizationId.New(),
                "Workspace organization",
                createdAtUtc).Value;
            Organization selectedOrganization = Organization.Create(
                OrganizationId.New(),
                "Selected organization",
                createdAtUtc).Value;
            Workspace workspace = Workspace.Create(
                TestIds.DefaultWorkspaceId,
                WorkspaceName.Create(TestValues.WorkspaceName).Value,
                WorkspaceType.Organization,
                workspaceOrganization.Id,
                createdAtUtc).Value;
            Artifact artifact = Artifact.Create(
                TestIds.DefaultArtifactId,
                workspace.Id,
                ArtifactTitle.Create(TestValues.ArtifactTitle).Value,
                ArtifactKindType.Instruction,
                ArtifactType.Markdown,
                createdAtUtc).Value;
            ArtifactRevision revision = artifact.CreateRevision(
                TestIds.DefaultArtifactRevisionId,
                ArtifactContent.Create(TestValues.ArtifactContent).Value,
                createdAtUtc).Value;
            BindingRepositorySpy bindingRepository = new();
            UnitOfWorkSpy unitOfWork = new();
            IMapper mapper = new MapperConfiguration(
                options => options.AddProfile<ApplicationMappingProfile>(),
                NullLoggerFactory.Instance).CreateMapper();
            SetBindingCommandHandler handler = new(
                new WorkspaceRepositorySpy { WorkspaceToReturn = workspace },
                new ArtifactRepositorySpy { ArtifactToReturn = artifact },
                new ArtifactRevisionRepositorySpy { ArtifactRevisionToReturn = revision },
                new OrganizationRepositorySpy { OrganizationToReturn = selectedOrganization },
                new ProjectRepositorySpy(),
                new ProjectTaskRepositorySpy(),
                bindingRepository,
                unitOfWork,
                new TestClockService(createdAtUtc),
                mapper);
            SetBindingCommand command = new(
                workspace.Id.Value,
                artifact.Id.Value,
                OrganizationId: selectedOrganization.Id.Value);

            DomainResult<BindingResponse> result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

            result.ShouldFailWith(BindingErrors.OrganizationWorkspaceMismatch);
            bindingRepository.UpsertedBinding.Should().BeNull();
            unitOfWork.SaveChangesCallCount.Should().Be(0);
        }
    }
}