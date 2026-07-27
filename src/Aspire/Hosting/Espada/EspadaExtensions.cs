namespace Aspire.Hosting.Espada;

internal static class EspadaExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        public void AddEspadaInfrastructure()
        {
            IResourceBuilder<ParameterResource> apiKey = builder
                .AddParameter(EspadaConstants.ParameterNames.ApiKey, secret: true)
                .WithDescription("Espada API key. Configure it in AppHost user secrets or as an Aspire parameter.", enableMarkdown: true);

            IResourceBuilder<ParameterResource> postgresPassword = builder
                .AddParameter(EspadaConstants.ParameterNames.PostgresPassword, () => ResolveParameter(builder, EspadaConstants.ParameterNames.PostgresPassword, EspadaConstants.ParameterDefaults.PostgresPassword), secret: true)
                .WithDescription("PostgreSQL password. Set manually for stable access from external DB tools (JetBrains, pgAdmin).", enableMarkdown: true);

            IResourceBuilder<PostgresServerResource> postgres = builder
                .AddPostgres(EspadaNames.Postgres)
                .WithImage("pgvector/pgvector")
                .WithImageTag("0.8.2-pg17")
                .WithPassword(postgresPassword)
                .WithHostPort(EspadaConstants.Ports.Postgres)
                .WithDataVolume(EspadaNames.PostgresData)
                .WithLifetime(ContainerLifetime.Persistent);

            postgres.WithPgAdmin(container => container
                .WithLifetime(ContainerLifetime.Persistent)
                .WithParentRelationship(postgres.Resource));

            IResourceBuilder<PostgresDatabaseResource> database = postgres.AddDatabase(EspadaNames.Database);

            IResourceBuilder<ProjectResource> migrations = builder
                .AddProject<Projects.Espada_Db>(EspadaNames.Migrations)
                .WithReference(database)
                .WithEnvironment(EspadaConstants.ConfigurationKeys.AspNetCoreEnvironment, EspadaConstants.ConfigurationValues.Development)
                .WithEnvironment(EspadaConstants.ConfigurationKeys.DotNetEnvironment, EspadaConstants.ConfigurationValues.Development)
                .WithArgs("migrate")
                .WaitFor(postgres);

            builder
                .AddProject<Projects.Espada_Api>(EspadaNames.Api)
                .WithReference(database)
                .WithEnvironment(EspadaConstants.ConfigurationKeys.ApiKey, apiKey)
                .WaitForCompletion(migrations);

            builder
                .AddProject<Projects.Espada_Daemon>(EspadaNames.Daemon)
                .WithReference(database)
                .WithEnvironment(EspadaConstants.ConfigurationKeys.ApiKey, apiKey)
                .WaitForCompletion(migrations);

            builder.AddViteApp(EspadaNames.Web, "../Espada.Web");
        }
    }

    private static string ResolveParameter(IDistributedApplicationBuilder builder, string parameterName, string fallback = "")
    {
        string? configuredParameterValue = builder.Configuration[$"{EspadaConstants.ConfigurationKeys.ParametersSectionPrefix}{parameterName}"];

        return !string.IsNullOrWhiteSpace(configuredParameterValue) ? configuredParameterValue : fallback;
    }
}
