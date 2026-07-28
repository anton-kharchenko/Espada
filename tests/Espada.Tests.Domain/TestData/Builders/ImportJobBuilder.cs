namespace Espada.Tests.Domain.TestData.Builders
{
    internal sealed class ImportJobBuilder
    {
        private ImportJobId _id = TestIds.DefaultImportJobId;

        private DateTimeOffset _requestedAtUtc = TestDates.ImportRequestedAtUtc;

        private SourceId _sourceId = TestIds.DefaultSourceId;

        private WorkspaceId _workspaceId = TestIds.DefaultWorkspaceId;

        public ImportJobBuilder WithId(ImportJobId id)
        {
            _id = id;
            return this;
        }

        public ImportJobBuilder ForSource(SourceId sourceId)
        {
            _sourceId = sourceId;
            return this;
        }

        public ImportJobBuilder InWorkspace(WorkspaceId workspaceId)
        {
            _workspaceId = workspaceId;
            return this;
        }

        public ImportJobBuilder RequestedAt(
            DateTimeOffset requestedAtUtc)
        {
            _requestedAtUtc = requestedAtUtc;
            return this;
        }

        public ImportJob BuildRequested()
        {
            return ImportJob.Request(_id, _sourceId, _workspaceId, _requestedAtUtc).ShouldSucceed();
        }

        public ImportJob BuildRequestedWithoutPendingEvents()
        {
            ImportJob importJob = BuildRequested();
            importJob.DequeueDomainEvents();

            return importJob;
        }

        public ImportJob BuildRunningWithoutPendingEvents()
        {
            ImportJob importJob = BuildRequestedWithoutPendingEvents();

            importJob.Start(TestDates.ImportStartedAtUtc).ShouldSucceed();
            importJob.DequeueDomainEvents();

            return importJob;
        }
    }
}