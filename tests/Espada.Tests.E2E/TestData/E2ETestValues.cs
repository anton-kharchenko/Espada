namespace Espada.Tests.E2E.TestData
{
    internal static class E2ETestValues
    {
        public const string ApiKeyHeader = "X-Espada-Api-Key";
        public const string ApiKey = "espada-e2e-tests-key";
        public const string PostgreSqlImage = "pgvector/pgvector:0.8.2-pg17";
        public const string PostgreSqlDatabase = "espada_e2e_tests";
        public const string PostgreSqlUsername = "postgres";
        public const string PostgreSqlPassword = "postgres";
    }
}