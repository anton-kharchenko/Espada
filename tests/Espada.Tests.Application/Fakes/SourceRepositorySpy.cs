using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;

namespace Espada.Tests.Application.Fakes;

internal sealed class SourceRepositorySpy : ISourceRepository
{
    public Source? AddedSource { get; private set; }

    public int AddCallCount { get; private set; }

    public CancellationToken ReceivedCancellationToken { get; private set; }

    public Task AddAsync(Source source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        AddedSource = source;
        AddCallCount++;
        ReceivedCancellationToken = cancellationToken;

        return Task.CompletedTask;
    }
}