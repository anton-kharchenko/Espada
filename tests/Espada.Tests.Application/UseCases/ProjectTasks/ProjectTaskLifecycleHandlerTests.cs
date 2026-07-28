using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Mappings;
using Espada.Application.UseCases.ProjectTasks.Commands.ArchiveProjectTask;
using Espada.Application.UseCases.ProjectTasks.Commands.CompleteProjectTask;
using Espada.Application.UseCases.ProjectTasks.Commands.CreateProjectTask;
using Espada.Application.UseCases.Projects.Common;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData;
using Microsoft.Extensions.Logging.Abstractions;

namespace Espada.Tests.Application.UseCases.ProjectTasks
{
    public sealed class ProjectTaskLifecycleHandlerTests
    {
        private readonly IMapper _mapper = new MapperConfiguration(
            options => options.AddProfile<ApplicationMappingProfile>(),
            NullLoggerFactory.Instance).CreateMapper();

        [Fact]
        public async Task Create_ShouldRejectProjectFromAnotherWorkspace()
        {
            ProjectRepositorySpy projectRepository = new()
            {
                ProjectToReturn = CreateProject(TestIds.AnotherWorkspaceId)
            };
            ProjectTaskRepositorySpy taskRepository = new();
            UnitOfWorkSpy unitOfWork = new();
            CreateProjectTaskCommandHandler handler = new(
                projectRepository,
                taskRepository,
                unitOfWork,
                new TestClockService(TestDates.ArtifactCreatedAtUtc),
                _mapper);
            CreateProjectTaskCommand command = new(
                TestIds.DefaultWorkspaceId.Value,
                projectRepository.ProjectToReturn.Id.Value,
                "Implement search");

            DomainResult<ProjectTaskResponse> result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

            result.ShouldFailWith(
                ProjectApplicationErrors.NotFoundInWorkspace(
                    command.ProjectId,
                    command.WorkspaceId));
            taskRepository.AddedTask.Should().BeNull();
        }

        [Fact]
        public async Task Complete_ShouldPersistLifecycleChange()
        {
            Project project = CreateProject(TestIds.DefaultWorkspaceId);
            ProjectTask task = project.CreateTask(
                TaskId.Create(Guid.NewGuid()),
                "Implement search",
                TestDates.ArtifactCreatedAtUtc).Value;
            ProjectTaskRepositorySpy repository = new() { TaskToReturn = task };
            UnitOfWorkSpy unitOfWork = new();
            CompleteProjectTaskCommandHandler handler = new(
                repository,
                unitOfWork,
                new TestClockService(TestDates.ArtifactSecondRevisionCreatedAtUtc),
                _mapper);

            DomainResult<ProjectTaskResponse> result = await handler.Handle(
                new CompleteProjectTaskCommand(task.WorkspaceId.Value, task.Id.Value),
                TestContext.Current.CancellationToken);

            ProjectTaskResponse response = result.ShouldSucceed();
            response.StatusTypeName.Should().Be(TaskStatusType.Completed.Name);
            unitOfWork.SaveChangesCallCount.Should().Be(1);
        }

        [Fact]
        public async Task Archive_ShouldPersistLifecycleChange()
        {
            Project project = CreateProject(TestIds.DefaultWorkspaceId);
            ProjectTask task = project.CreateTask(
                TaskId.Create(Guid.NewGuid()),
                "Implement search",
                TestDates.ArtifactCreatedAtUtc).Value;
            ProjectTaskRepositorySpy repository = new() { TaskToReturn = task };
            UnitOfWorkSpy unitOfWork = new();
            ArchiveProjectTaskCommandHandler handler = new(
                repository,
                unitOfWork,
                new TestClockService(TestDates.ArtifactSecondRevisionCreatedAtUtc),
                _mapper);

            DomainResult<ProjectTaskResponse> result = await handler.Handle(
                new ArchiveProjectTaskCommand(task.WorkspaceId.Value, task.Id.Value),
                TestContext.Current.CancellationToken);

            ProjectTaskResponse response = result.ShouldSucceed();
            response.StatusTypeName.Should().Be(TaskStatusType.Archived.Name);
            unitOfWork.SaveChangesCallCount.Should().Be(1);
        }

        private static Project CreateProject(WorkspaceId workspaceId)
        {
            return Project.Create(
                ProjectId.Create(Guid.NewGuid()),
                workspaceId,
                "Espada",
                "https://github.com/example/espada",
                ["C:/src/espada"],
                TestDates.ArtifactCreatedAtUtc).Value;
        }
    }
}