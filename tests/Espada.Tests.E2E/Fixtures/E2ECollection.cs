namespace Espada.Tests.E2E.Fixtures;

[CollectionDefinition(Name)]
public sealed class E2ECollection : ICollectionFixture<EspadaE2EFactory>
{
    public const string Name = "Espada API E2E";
}