using Espada.Domain.SeedWork;

namespace Espada.Domain.Enums
{
    public sealed class AgentVendorType(int id, string name) : Enumeration(id, name)
    {
        public static readonly AgentVendorType Codex = new(1, nameof(Codex));

        public static readonly AgentVendorType Claude = new(2, nameof(Claude));

        public static readonly AgentVendorType Gemini = new(3, nameof(Gemini));

        public static readonly AgentVendorType Grok = new(4, nameof(Grok));

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