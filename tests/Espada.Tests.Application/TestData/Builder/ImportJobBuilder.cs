using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.TestData.Builder
{
    internal sealed class ImportJobBuilder
    {
        private ImportJobId _id = TestIds.DefaultImportJobId;

        private WorkspaceId _workspaceId = TestIds.DefaultWorkspaceId;

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

        public ImportJob BuildRunningWithoutPendingEvents(DateTimeOffset? startedAtUtc = null)
        {
            ImportJob importJob = BuildWithoutPendingEvents();

            DomainResult startResult = importJob.Start(startedAtUtc ?? TestDates.ImportStartedAtUtc);

            if (startResult.IsFailure)
            {
                throw new InvalidOperationException("ImportJobBuilder could not start import job: " + startResult.Error.Code);
            }

            importJob.DequeueDomainEvents();

            return importJob;
        }

        public ImportJob BuildSucceededWithoutPendingEvents(DateTimeOffset? completedAtUtc = null, ArtifactId? artifactId = null, ArtifactRevisionId? artifactRevisionId = null)
        {
            ImportJob importJob = BuildRunningWithoutPendingEvents();

            DomainResult completeResult = importJob.Complete(artifactId ?? TestIds.DefaultArtifactId, artifactRevisionId ?? TestIds.DefaultArtifactRevisionId, completedAtUtc ?? TestDates.ImportCompletedAtUtc);

            if (completeResult.IsFailure)
            {
                throw new InvalidOperationException($"ImportJobBuilder could not complete import job: {completeResult.Error.Code} — {completeResult.Error.Description}");
            }

            importJob.DequeueDomainEvents();

            return importJob;
        }

        public ImportJob BuildFailedWithoutPendingEvents(string failureCode = TestValues.ImportFailureCode, string failureReason = TestValues.ImportFailureReason, DateTimeOffset? failedAtUtc = null)
        {
            ImportJob importJob = BuildRunningWithoutPendingEvents();

            DomainResult<ImportFailure> failureResult = ImportFailure.Create(failureCode, failureReason);

            if (failureResult.IsFailure)
            {
                throw new InvalidOperationException($"ImportJobBuilder received an invalid failure: {failureResult.Error.Code} — {failureResult.Error.Description}");
            }

            DomainResult failResult = importJob.Fail(failureResult.Value, failedAtUtc ?? TestDates.ImportFailedAtUtc);

            if (failResult.IsFailure)
            {
                throw new InvalidOperationException($"ImportJobBuilder could not fail import job: {failResult.Error.Code} — {failResult.Error.Description}");
            }

            importJob.DequeueDomainEvents();

            return importJob;
        }

        public ImportJob BuildCancelledFromRequestedWithoutPendingEvents(DateTimeOffset? cancelledAtUtc = null)
        {
            ImportJob importJob = BuildWithoutPendingEvents();

            DomainResult cancelResult = importJob.Cancel(cancelledAtUtc ?? TestDates.ImportCancelledAtUtc);

            if (cancelResult.IsFailure)
            {
                throw new InvalidOperationException($"ImportJobBuilder could not cancel requested import job: {cancelResult.Error.Code} — {cancelResult.Error.Description}");
            }

            importJob.DequeueDomainEvents();

            return importJob;
        }

        public ImportJob BuildCancelledFromRunningWithoutPendingEvents(DateTimeOffset? cancelledAtUtc = null)
        {
            ImportJob importJob = BuildRunningWithoutPendingEvents();

            DomainResult cancelResult = importJob.Cancel(cancelledAtUtc ?? TestDates.ImportCancelledAtUtc);

            if (cancelResult.IsFailure)
            {
                throw new InvalidOperationException($"ImportJobBuilder could not cancel running import job: {cancelResult.Error.Code} — {cancelResult.Error.Description}");
            }

            importJob.DequeueDomainEvents();

            return importJob;
        }
    }
}