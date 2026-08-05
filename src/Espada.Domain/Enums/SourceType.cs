using Espada.Domain.SeedWork;

namespace Espada.Domain.Enums
{
    public sealed class SourceType(int id, string name) : Enumeration(id, name)
    {
        public static readonly SourceType File = new(1, nameof(File));

        public static readonly SourceType WebPage = new(2, nameof(WebPage));

        public static readonly SourceType PlainText = new(3, nameof(PlainText));

        public static readonly SourceType Conversation = new(4, nameof(Conversation));

        public static readonly SourceType Connector = new(5, nameof(Connector));

        public static readonly SourceType Repository = new(6, nameof(Repository));
        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}