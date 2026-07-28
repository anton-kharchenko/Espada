using Espada.Domain.Enums;

namespace Espada.Domain.ValueObjects.SourceDefinitions
{
    public sealed record LegacySourceDefinition(int SourceTypeId, string Locator) : SourceDefinition
    {
        public override SourceType SourceType => SourceTypeId switch
        {
            1 => SourceType.File,
            2 => SourceType.WebPage,
            3 => SourceType.PlainText,
            4 => SourceType.Conversation,
            5 => SourceType.Connector,
            _ => throw new InvalidOperationException($"Unknown source type ID '{SourceTypeId}'.")
        };

        public override string CanonicalLocator => Locator;
    }
}