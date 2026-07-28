using Espada.Domain.SeedWork;

namespace Espada.Domain.Enums;

public sealed class ChunkBatchStatusType(int id, string name) : Enumeration(id, name)
{
    public static readonly ChunkBatchStatusType Requested = new(1, nameof(Requested));

    public static readonly ChunkBatchStatusType Running = new(2, nameof(Running));

    public static readonly ChunkBatchStatusType Succeeded = new(3, nameof(Succeeded));

    public static readonly ChunkBatchStatusType Failed = new(4, nameof(Failed));
}