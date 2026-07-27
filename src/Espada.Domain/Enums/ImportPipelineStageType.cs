using Espada.Domain.SeedWork;

namespace Espada.Domain.Enums;

public sealed class ImportPipelineStageType(int id, string name) : Enumeration(id, name)
{
    public static readonly ImportPipelineStageType Start = new(1, nameof(Start));

    public static readonly ImportPipelineStageType Read = new(2, nameof(Read));

    public static readonly ImportPipelineStageType Parse = new(3, nameof(Parse));

    public static readonly ImportPipelineStageType MaterializeArtifact =
        new(4, nameof(MaterializeArtifact));

    public static readonly ImportPipelineStageType Chunk = new(5, nameof(Chunk));

    public static readonly ImportPipelineStageType EmbedAndIndex =
        new(6, nameof(EmbedAndIndex));

    public static readonly ImportPipelineStageType Complete = new(7, nameof(Complete));

    public override bool Equals(object? obj) => base.Equals(obj);

    public override int GetHashCode() => base.GetHashCode();
}