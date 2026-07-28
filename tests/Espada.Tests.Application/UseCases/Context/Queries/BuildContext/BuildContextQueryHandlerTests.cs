using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Mappings;
using Espada.Application.Models;
using Espada.Application.Services;
using Espada.Application.UseCases.Context.Queries.BuildContext;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData;
using Microsoft.Extensions.Logging.Abstractions;
using Espada.Application.Constants;

namespace Espada.Tests.Application.UseCases.Context.Queries.BuildContext
{
    public sealed class BuildContextQueryHandlerTests
    {
        private static readonly DateTimeOffset CreatedAtUtc =
            new(2026, 7, 28, 14, 0, 0, TimeSpan.Zero);

        [Fact]
        public async Task Handle_WithProjectContext_ShouldDeriveRepositoryAndNormalizeInput()
        {
            Workspace workspace = CreateWorkspace();
            Project project = CreateProject(workspace);
            ContextCandidateRecord candidate = CreateCandidate(
                workspace,
                project);
            ContextCandidateStoreSpy candidateStore = new() { CandidatesToReturn = [candidate] };
            BuildContextQueryHandler handler = CreateHandler(
                workspace,
                project,
                null,
                candidateStore);
            BuildContextQuery query = new(
                workspace.Id.Value,
                project.Id.Value,
                null,
                " src\\Espada.Application\\ ",
                " feature/context ",
                "CoDeX",
                4_096);

            DomainResult<BuildContextResponse> result = await handler.Handle(
                query,
                TestContext.Current.CancellationToken);

            BuildContextResponse response = result.ShouldSucceed();
            Assert.Equal(project.CanonicalRemoteUri, response.RepositoryCanonicalUri);
            Assert.Equal("src/Espada.Application", response.RepositoryRelativePath);
            Assert.Equal("feature/context", response.Branch);
            Assert.Equal(ContextAgentConstants.Codex, response.Agent);
            Assert.Single(response.IncludedItems);
        }

        [Fact]
        public async Task Handle_ShouldForwardCancellationToCandidateStore()
        {
            Workspace workspace = CreateWorkspace();
            ContextCandidateStoreSpy candidateStore = new();
            BuildContextQueryHandler handler = CreateHandler(
                workspace,
                null,
                null,
                candidateStore);
            using CancellationTokenSource source = new();

            DomainResult<BuildContextResponse> result = await handler.Handle(
                new BuildContextQuery(
                    workspace.Id.Value,
                    null,
                    null,
                    null,
                    null,
                    ContextAgentConstants.Generic,
                    1_024),
                source.Token);

            result.ShouldSucceed();
            Assert.Equal(source.Token, candidateStore.ReceivedCancellationToken);
            Assert.Equal(workspace.Id, candidateStore.ReceivedWorkspaceId);
        }

        [Fact]
        public async Task Handle_WithMissingProjectForScopedInput_ShouldRejectRequest()
        {
            Workspace workspace = CreateWorkspace();
            BuildContextQueryHandler handler = CreateHandler(
                workspace,
                null,
                null,
                new ContextCandidateStoreSpy());

            DomainResult<BuildContextResponse> result = await handler.Handle(
                new BuildContextQuery(
                    workspace.Id.Value,
                    null,
                    null,
                    "src",
                    null,
                    ContextAgentConstants.Codex,
                    1_024),
                TestContext.Current.CancellationToken);

            result.ShouldFailWith(ContextApplicationErrors.ProjectRequired);
        }

        [Fact]
        public async Task Handle_WithCrossWorkspaceProject_ShouldHideIt()
        {
            Workspace workspace = CreateWorkspace();
            Project foreignProject = CreateProject(CreateWorkspace());
            BuildContextQueryHandler handler = CreateHandler(
                workspace,
                foreignProject,
                null,
                new ContextCandidateStoreSpy());

            DomainResult<BuildContextResponse> result = await handler.Handle(
                new BuildContextQuery(
                    workspace.Id.Value,
                    foreignProject.Id.Value,
                    null,
                    null,
                    null,
                    ContextAgentConstants.Codex,
                    1_024),
                TestContext.Current.CancellationToken);

            result.ShouldFailWith(
                ProjectApplicationErrors.NotFoundInWorkspace(
                    foreignProject.Id.Value,
                    workspace.Id.Value));
        }

        [Fact]
        public async Task Handle_WithTaskFromAnotherProject_ShouldHideIt()
        {
            Workspace workspace = CreateWorkspace();
            Project project = CreateProject(workspace);
            Project otherProject = CreateProject(workspace);
            ProjectTask task = otherProject.CreateTask(
                TaskId.New(),
                "Other",
                CreatedAtUtc).Value;
            BuildContextQueryHandler handler = CreateHandler(
                workspace,
                project,
                task,
                new ContextCandidateStoreSpy());

            DomainResult<BuildContextResponse> result = await handler.Handle(
                new BuildContextQuery(
                    workspace.Id.Value,
                    project.Id.Value,
                    task.Id.Value,
                    null,
                    null,
                    ContextAgentConstants.Codex,
                    1_024),
                TestContext.Current.CancellationToken);

            result.ShouldFailWith(
                ProjectTaskApplicationErrors.NotFoundInProject(
                    task.Id.Value,
                    project.Id.Value));
        }

        [Theory]
        [MemberData(nameof(BuildContextPathTestData.InvalidRepositoryRelativePaths), MemberType = typeof(BuildContextPathTestData))]
        public async Task Handle_WithInvalidRepositoryRelativePath_ShouldRejectRequest(
            string path)
        {
            Workspace workspace = CreateWorkspace();
            Project project = CreateProject(workspace);
            BuildContextQueryHandler handler = CreateHandler(
                workspace,
                project,
                null,
                new ContextCandidateStoreSpy());

            DomainResult<BuildContextResponse> result = await handler.Handle(
                new BuildContextQuery(
                    workspace.Id.Value,
                    project.Id.Value,
                    null,
                    path,
                    null,
                    ContextAgentConstants.Codex,
                    1_024),
                TestContext.Current.CancellationToken);

            result.ShouldFailWith(
                ContextApplicationErrors.RepositoryRelativePathInvalid);
        }

        private static BuildContextQueryHandler CreateHandler(
            Workspace workspace,
            Project? project,
            ProjectTask? task,
            ContextCandidateStoreSpy candidateStore)
        {
            IMapper mapper = new MapperConfiguration(
                options => options.AddProfile<ApplicationMappingProfile>(),
                NullLoggerFactory.Instance).CreateMapper();
            return new BuildContextQueryHandler(
                new WorkspaceRepositorySpy { WorkspaceToReturn = workspace },
                new ProjectRepositorySpy { ProjectToReturn = project },
                new ProjectTaskRepositorySpy { TaskToReturn = task },
                candidateStore,
                new ContextResolver(),
                mapper);
        }

        private static Workspace CreateWorkspace()
        {
            return Workspace.Create(
                WorkspaceId.New(),
                WorkspaceName.Create("Context workspace").Value,
                WorkspaceType.Personal,
                null,
                CreatedAtUtc).Value;
        }

        private static Project CreateProject(Workspace workspace)
        {
            return Project.Create(
                ProjectId.New(),
                workspace.Id,
                "Espada",
                $"https://example.test/{Guid.NewGuid():N}.git",
                [],
                CreatedAtUtc).Value;
        }

        private static ContextCandidateRecord CreateCandidate(
            Workspace workspace,
            Project project)
        {
            Artifact artifact = Artifact.Create(
                ArtifactId.New(),
                workspace.Id,
                ArtifactTitle.Create("Context rule").Value,
                ArtifactKindType.Instruction,
                ArtifactType.Markdown,
                CreatedAtUtc).Value;
            ArtifactRevision revision = artifact.CreateRevision(
                ArtifactRevisionId.New(),
                ArtifactContent.Create("Use canonical context.").Value,
                CreatedAtUtc).Value;
            InstructionRule rule = artifact.CreateInstructionRule(
                revision,
                RuleKey.Create("context.canonical").Value,
                "Use canonical context.",
                ContextPriority.Neutral).Value;
            Binding binding = artifact.CreateBinding(
                BindingId.New(),
                revision,
                workspace,
                null,
                project,
                project.CanonicalRemoteUri,
                "src",
                "feature/context",
                null,
                "codex",
                CreatedAtUtc).Value;
            return new ContextCandidateRecord(
                binding,
                artifact,
                revision,
                [rule],
                [],
                null,
                false);
        }
    }
}