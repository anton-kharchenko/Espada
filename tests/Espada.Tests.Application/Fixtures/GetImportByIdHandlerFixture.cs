using AutoMapper;
using Espada.Application.Mappings;
using Espada.Application.UseCases.Imports.Queries.GetImportById;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;
using Microsoft.Extensions.Logging.Abstractions;

namespace Espada.Tests.Application.Fixtures
{
    internal sealed class GetImportByIdHandlerFixture
    {
        private readonly IMapper _mapper =
            new MapperConfiguration(options => options.AddProfile<ApplicationMappingProfile>(),
                NullLoggerFactory.Instance).CreateMapper();

        public ImportJobRepositorySpy ImportJobRepository { get; } = new();

        public GetImportByIdQueryHandler CreateHandler()
        {
            return new GetImportByIdQueryHandler(ImportJobRepository, new EmptyJobQueue(), _mapper);
        }

        public ImportJob GivenRequestedImportExists(WorkspaceId? workspaceId = null)
        {
            ImportJob importJob = new ImportJobBuilder()
                .InWorkspace(workspaceId ?? TestIds.DefaultWorkspaceId)
                .BuildWithoutPendingEvents();

            ImportJobRepository.ImportJobToReturn = importJob;

            return importJob;
        }

        public ImportJob GivenRunningImportExists(WorkspaceId? workspaceId = null)
        {
            ImportJob importJob = new ImportJobBuilder()
                .InWorkspace(workspaceId ?? TestIds.DefaultWorkspaceId)
                .BuildRunningWithoutPendingEvents();

            ImportJobRepository.ImportJobToReturn = importJob;

            return importJob;
        }

        public ImportJob GivenSucceededImportExists(WorkspaceId? workspaceId = null)
        {
            ImportJob importJob = new ImportJobBuilder()
                .InWorkspace(workspaceId ?? TestIds.DefaultWorkspaceId)
                .BuildSucceededWithoutPendingEvents();

            ImportJobRepository.ImportJobToReturn = importJob;

            return importJob;
        }

        public ImportJob GivenFailedImportExists(WorkspaceId? workspaceId = null)
        {
            ImportJob importJob = new ImportJobBuilder()
                .InWorkspace(workspaceId ?? TestIds.DefaultWorkspaceId)
                .BuildFailedWithoutPendingEvents();

            ImportJobRepository.ImportJobToReturn = importJob;

            return importJob;
        }

        public ImportJob GivenCancelledImportExists(WorkspaceId? workspaceId = null)
        {
            ImportJob importJob = new ImportJobBuilder()
                .InWorkspace(workspaceId ?? TestIds.DefaultWorkspaceId)
                .BuildCancelledFromRequestedWithoutPendingEvents();

            ImportJobRepository.ImportJobToReturn = importJob;

            return importJob;
        }

        public void GivenImportDoesNotExist()
        {
            ImportJobRepository.ImportJobToReturn = null;
        }
    }
}