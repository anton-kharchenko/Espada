using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class MemoryMetadataRepositorySpy : IMemoryMetadataRepository
    {
        public MemoryMetadata? AddedMetadata { get; private set; }
        public MemoryMetadata? MetadataToReturn { get; set; }
        public bool IsSuperseded { get; set; }
        public CancellationToken AddCancellationToken { get; private set; }
        public CancellationToken GetCancellationToken { get; private set; }
        public CancellationToken SupersededCancellationToken { get; private set; }

        public Task AddAsync(
            MemoryMetadata metadata,
            CancellationToken cancellationToken = default)
        {
            AddedMetadata = metadata;
            AddCancellationToken = cancellationToken;
            return Task.CompletedTask;
        }

        public Task<MemoryMetadata?> GetByIdAsync(
            MemoryId memoryId,
            CancellationToken cancellationToken = default)
        {
            GetCancellationToken = cancellationToken;
            return Task.FromResult(MetadataToReturn);
        }

        public Task<bool> IsSupersededAsync(
            MemoryId memoryId,
            CancellationToken cancellationToken = default)
        {
            SupersededCancellationToken = cancellationToken;
            return Task.FromResult(IsSuperseded);
        }
    }
}