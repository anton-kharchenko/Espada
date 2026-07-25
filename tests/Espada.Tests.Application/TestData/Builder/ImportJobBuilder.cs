using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.TestData.Builder
{
    internal sealed class ImportJobBuilder
    {
        private ImportJobId _id = TestIds.DefaultImportJobId;

        private WorkspaceId _workspaceId = TestIds.WorkspaceId;

        private SourceId _sourceId = TestIds.SourceId;

        private DateTimeOffset _requestedAtUtc = TestDates.ImportRequestedAtUtc;

        public ImportJobBuilder WithId(ImportJobId id)
        {
            _id = id;
            return this;
        }

        public ImportJobBuilder InWorkspace(WorkspaceId workspaceId)
        {
            _workspaceId = workspaceId;
            return this;
        }

        public ImportJobBuilder ForSource(SourceId sourceId)
        {
            _sourceId = sourceId;
            return this;
        }

        public ImportJobBuilder RequestedAt(DateTimeOffset requestedAtUtc)
        {
            _requestedAtUtc = requestedAtUtc;
            return this;
        }

        public DomainResult<ImportJob> BuildResult() => ImportJob.Request(_id, _sourceId, _workspaceId, _requestedAtUtc);

        public ImportJob Build()
        {
            DomainResult<ImportJob> result = BuildResult();

            return result.IsFailure ? throw new InvalidOperationException($"ImportJobBuilder produced an invalid import job: {result.Error.Code} — {result.Error.Description}") : result.Value;
        }

        public ImportJob BuildWithoutPendingEvents()
        {
            ImportJob importJob = Build();

            importJob.DequeueDomainEvents();

            return importJob;
        }
    }
}