using Espada.Application.Contracts.Persistence;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class UnitOfWorkSpy : IUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }

        public CancellationToken ReceivedCancellationToken { get; private set; }

        public int SaveChangesResult { get; set; } = 1;

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            ReceivedCancellationToken = cancellationToken;

            return Task.FromResult(SaveChangesResult);
        }
    }
}