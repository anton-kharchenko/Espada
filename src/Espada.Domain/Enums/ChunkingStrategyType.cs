using Espada.Domain.SeedWork;

namespace Espada.Domain.Enums
{
    public sealed class ChunkingStrategyType(int id, string name) : Enumeration(id, name)
    {
        public static readonly ChunkingStrategyType FixedSize = new(1, nameof(FixedSize));

        public static readonly ChunkingStrategyType Recursive = new(2, nameof(Recursive));

        public static readonly ChunkingStrategyType Markdown = new(3, nameof(Markdown));

        public static readonly ChunkingStrategyType Semantic = new(4, nameof(Semantic));

        public static readonly ChunkingStrategyType Code = new(5, nameof(Code));

        public static readonly ChunkingStrategyType Custom = new(6, nameof(Custom));
    }
}