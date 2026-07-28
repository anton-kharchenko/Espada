using Espada.Application.UseCases.Imports.Commands.FailImport;

namespace Espada.Tests.Application.TestData.Builder
{
    internal sealed class FailImportCommandBuilder
    {
        private string? _failureCode = TestValues.ImportFailureCode;

        private string? _failureReason = TestValues.ImportFailureReason;

        private Guid _importJobId = TestIds.DefaultImportJobId.Value;
        private Guid _workspaceId = TestIds.DefaultWorkspaceId.Value;

        public FailImportCommandBuilder InWorkspace(Guid workspaceId)
        {
            _workspaceId = workspaceId;
            return this;
        }

        public FailImportCommandBuilder ForImportJob(Guid importJobId)
        {
            _importJobId = importJobId;
            return this;
        }

        public FailImportCommandBuilder WithFailureCode(string? failureCode)
        {
            _failureCode = failureCode;
            return this;
        }

        public FailImportCommandBuilder WithFailureReason(string? failureReason)
        {
            _failureReason = failureReason;
            return this;
        }

        public FailImportCommand Build()
        {
            return new FailImportCommand(_workspaceId, _importJobId, _failureCode!, _failureReason!);
        }
    }
}