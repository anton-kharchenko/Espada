namespace Espada.Tests.Integration.Fixtures
{
    [CollectionDefinition(Name)]
    public sealed class PostgreSqlIntegrationCollection : ICollectionFixture<PostgreSqlDatabaseFixture>
    {
        public const string Name = "PostgreSQL integration";
    }
}