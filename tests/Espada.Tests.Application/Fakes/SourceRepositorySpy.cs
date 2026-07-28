using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class SourceRepositorySpy : ISourceRepository
    {
        public Source? AddedSource { get; private set; }

        public Source? SourceToReturn { get; set; }

        public IReadOnlyList<Source> SourcesToReturn { get; set; } = [];

        public int AddCallCount { get; private set; }

        public int GetByIdCallCount { get; private set; }

        public SourceId? ReceivedSourceId { get; private set; }

        public CancellationToken AddCancellationToken { get; private set; }

        public CancellationToken GetByIdCancellationToken { get; private set; }

        public Task AddAsync(Source source, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);

            AddedSource = source;
            AddCallCount++;
            AddCancellationToken = cancellationToken;

            return Task.CompletedTask;
        }

        public Task<Source?> GetByIdAsync(SourceId sourceId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(sourceId);

            ReceivedSourceId = sourceId;
            GetByIdCallCount++;
            GetByIdCancellationToken = cancellationToken;

            return Task.FromResult(SourceToReturn);
        }

        public Task<IReadOnlyList<Source>> ListByWorkspaceIdAsync(
            WorkspaceId workspaceId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(SourcesToReturn);
        }
    }
}
