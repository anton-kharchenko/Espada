using Espada.Application.Policies.Billing;
using Espada.Application.UseCases.Imports.Commands.RequestImport;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.Fixtures
{
    internal sealed class RequestImportHandlerFixture
    {
        public SourceRepositorySpy SourceRepository { get; } = new();

        public ImportJobRepositorySpy ImportJobRepository { get; } = new();

        public UnitOfWorkSpy UnitOfWork { get; } = new();

        public TestClockService ClockService { get; } = new(TestDates.ImportRequestedAtUtc);

        public TestEmbeddingModelDefaults EmbeddingModelDefaults { get; } = new();

        public RequestImportCommandHandler CreateHandler()
        {
            return new RequestImportCommandHandler(
                SourceRepository,
                ImportJobRepository,
                UnitOfWork,
                ClockService,
                new AllowImportAdmissionPolicy(),
                EmbeddingModelDefaults);
        }

        public Source GivenSourceExists(WorkspaceId? workspaceId = null)
        {
            Source source = new SourceBuilder().InWorkspace(workspaceId ?? TestIds.DefaultWorkspaceId)
                .BuildWithoutPendingEvents();

            SourceRepository.SourceToReturn = source;

            return source;
        }

        public void GivenSourceDoesNotExist()
        {
            SourceRepository.SourceToReturn = null;
        }

        public ImportJob GivenImportWithSameRequestExists(RequestImportCommand command)
        {
            string fingerprint = RequestImportFingerprint.Create(command.SourceId, command.Options);
            ImportJob importJob = ImportJob.Request(
                ImportJobId.New(),
                SourceId.Create(command.SourceId),
                WorkspaceId.Create(command.WorkspaceId),
                TestDates.ImportRequestedAtUtc,
                command.IdempotencyKey,
                fingerprint,
                RequestImportFingerprint.SerializeOptions(command.Options)).ShouldSucceed();
            importJob.DequeueDomainEvents();
            ImportJobRepository.ImportJobByIdempotencyKeyToReturn = importJob;
            return importJob;
        }
    }
}