using Espada.Domain.SeedWork;

namespace Espada.Domain.Enums;

public sealed class ArtifactType(int id, string name) : Enumeration(id, name)
{
    public static readonly ArtifactType Text = new(1, nameof(Text));

    public static readonly ArtifactType Markdown = new(2, nameof(Markdown));

    public static readonly ArtifactType File = new(3, nameof(File));

    public static readonly ArtifactType WebPage = new(4, nameof(WebPage));

    public static readonly ArtifactType Conversation = new(5, nameof(Conversation));

}