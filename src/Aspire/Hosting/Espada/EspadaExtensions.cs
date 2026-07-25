namespace Aspire.Hosting.Espada;

internal static class EspadaExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        public void AddEspadaInfrastructure()
        {
            IResourceBuilder<ParameterResource> postgresPassword = builder
                .AddParameter(EspadaConstants.ParameterNames.PostgresPassword, () => ResolveParameter(builder, EspadaConstants.ParameterNames.PostgresPassword, EspadaConstants.ParameterDefaults.PostgresPassword), secret: true)
                .WithDescription("PostgreSQL password. Set manually for stable access from external DB tools (JetBrains, pgAdmin).", enableMarkdown: true);

            IResourceBuilder<PostgresServerResource> postgres = builder
                .AddPostgres(EspadaNames.Postgres)
                .WithPassword(postgresPassword)
                .WithHostPort(EspadaConstants.Ports.Postgres)
                .WithDataVolume(EspadaNames.PostgresData)
                .WithLifetime(ContainerLifetime.Persistent);

            postgres.WithPgAdmin(container => container
                .WithLifetime(ContainerLifetime.Persistent)
                .WithParentRelationship(postgres.Resource));

            IResourceBuilder<PostgresDatabaseResource> database = postgres.AddDatabase(EspadaNames.Database);

            builder
                .AddProject<Projects.Espada_Db>(EspadaNames.Migrations)
                .WithReference(database)
                .WithEnvironment(EspadaConstants.ConfigurationKeys.AspNetCoreEnvironment, EspadaConstants.ConfigurationValues.Development)
                .WithEnvironment(EspadaConstants.ConfigurationKeys.DotNetEnvironment, EspadaConstants.ConfigurationValues.Development)
                .WithArgs("migrate")
                .WaitFor(postgres);
        }
    }

    private static string ResolveParameter(IDistributedApplicationBuilder builder, string parameterName, string fallback = "")
    {
        string? configuredParameterValue = builder.Configuration[$"{EspadaConstants.ConfigurationKeys.ParametersSectionPrefix}{parameterName}"];

        return !string.IsNullOrWhiteSpace(configuredParameterValue) ? configuredParameterValue : fallback;
    }
}
