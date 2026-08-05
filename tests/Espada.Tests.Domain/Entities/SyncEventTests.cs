using Espada.Domain.Entities;
using Espada.Domain.Errors;

namespace Espada.Tests.Domain.Entities
{
    public sealed class SyncEventTests
    {
        [Fact]
        public void Create_WithNonPositiveSequence_ShouldReturnFailure()
        {
            DomainResult<SyncEvent> result = SyncEvent.Create(SyncEventId.New(), DeviceId.New(), 0,
                WorkspaceId.New(), "instruction", Guid.NewGuid(), "upsert", null, DateTimeOffset.UtcNow,
                "instruction.v1", "{}", "sha256:abc");

            result.ShouldFailWith(SyncEventErrors.SequenceOutOfRange);
        }
    }
}
