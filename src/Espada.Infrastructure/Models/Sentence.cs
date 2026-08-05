namespace Espada.Infrastructure.Models
{
    internal sealed record Sentence(
        string Content,
        int Start,
        int End);
}