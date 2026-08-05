using Aspire.Hosting.Espada.Constants;
using Microsoft.Extensions.Configuration;
using Projects;

namespace Aspire.Hosting.Espada
{
    internal static class EspadaExtensions
    {
        extension(IDistributedApplicationBuilder builder)
        {
            public void AddEspadaInfrastructure()
            {
                IResourceBuilder<ParameterResource> apiKey = builder
                    .AddParameter(EspadaParameterNameConstants.ApiKey, true)
                    .WithDescription(
                        "Espada API key. Configure it in AppHost user secrets or as an Aspire parameter.",
                        true);

                IResourceBuilder<ParameterResource> stripeApiKey = builder
                    .AddParameter(EspadaParameterNameConstants.StripeApiKey, true)
                    .WithDescription(
                        "Restricted Stripe API key for local billing and webhook forwarding. " +
                        "Configure it as an Aspire parameter; never commit the key.",
                        true);

                IResourceBuilder<ParameterResource> postgresPassword = builder
                    .AddParameter(
                        EspadaParameterNameConstants.PostgresPassword,
                        () => ResolveParameter(
                            builder,
                            EspadaParameterNameConstants.PostgresPassword,
                            EspadaParameterDefaultConstants.PostgresPassword),
                        secret: true)
                    .WithDescription(
                        "PostgreSQL password. Set manually for stable access from external DB tools (JetBrains, pgAdmin).",
                        true);

                IResourceBuilder<PostgresServerResource> postgres = builder
                    .AddPostgres(EspadaResourceNameConstants.Postgres)
                    .WithImage("pgvector/pgvector")
                    .WithImageTag("0.8.2-pg17")
                    .WithPassword(postgresPassword)
                    .WithHostPort(EspadaPortConstants.Postgres)
                    .WithDataVolume(EspadaResourceNameConstants.PostgresData)
                    .WithLifetime(ContainerLifetime.Persistent);

                postgres.WithPgAdmin(container => container
                    .WithLifetime(ContainerLifetime.Persistent)
                    .WithParentRelationship(postgres.Resource));

                IResourceBuilder<PostgresDatabaseResource> database =
                    postgres.AddDatabase(EspadaResourceNameConstants.Database);
                string blobRoot = Path.Join(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Espada",
                    "aspire-blobs");

                IResourceBuilder<ProjectResource> migrations = builder
                    .AddProject<Espada_Db>(EspadaResourceNameConstants.Migrations)
                    .WithReference(database)
                    .WithEnvironment(
                        EspadaConfigurationKeyConstants.AspNetCoreEnvironment,
                        EspadaConfigurationValueConstants.Development)
                    .WithEnvironment(
                        EspadaConfigurationKeyConstants.DotNetEnvironment,
                        EspadaConfigurationValueConstants.Development)
                    .WithArgs("migrate")
                    .WaitFor(postgres);

                IResourceBuilder<ProjectResource> api = builder
                    .AddProject<Espada_Api>(EspadaResourceNameConstants.Api)
                    .WithReference(database)
                    .WithEnvironment(EspadaConfigurationKeyConstants.ApiKey, apiKey)
                    .WithEnvironment(EspadaConfigurationKeyConstants.BlobRoot, blobRoot)
                    .WaitForCompletion(migrations);

                builder
                    .AddProject<Espada_Daemon>(EspadaResourceNameConstants.Daemon)
                    .WithReference(database)
                    .WithEnvironment(EspadaConfigurationKeyConstants.ApiKey, apiKey)
                    .WithEnvironment(EspadaConfigurationKeyConstants.BlobRoot, blobRoot)
                    .WithEnvironment(EspadaConfigurationKeyConstants.LocalRuntimeEnabled, "false")
                    .WaitForCompletion(migrations);

                builder
                    .AddProject<Espada_Mcp>(EspadaResourceNameConstants.Mcp)
                    .WithReference(database)
                    .WithEnvironment(EspadaConfigurationKeyConstants.BlobRoot, blobRoot)
                    .WithHttpEndpoint(
                        port: EspadaPortConstants.Mcp,
                        name: "http")
                    .WaitForCompletion(migrations);

                IResourceBuilder<ProjectResource> worker = builder
                    .AddProject<Espada_Worker>(EspadaResourceNameConstants.Worker)
                    .WithReference(database)
                    .WithEnvironment(EspadaConfigurationKeyConstants.BlobRoot, blobRoot)
                    .WaitForCompletion(migrations);

                IResourceBuilder<StripeResource> stripe = builder.AddStripe(
                    EspadaResourceNameConstants.Stripe,
                    stripeApiKey);
                if (builder.Configuration.GetValue<bool>(
                        EspadaConfigurationKeyConstants.AppHostDisableStripe))
                {
                    stripe.WithExplicitStart();
                    api.WithEnvironment(
                        EspadaConfigurationKeyConstants.BillingStripeWebhookSecret,
                        EspadaConfigurationValueConstants.StripeDisabledWebhookSecret);
                    worker.WithEnvironment(
                        EspadaConfigurationKeyConstants.BillingStripeWebhookSecret,
                        EspadaConfigurationValueConstants.StripeDisabledWebhookSecret);
                }
                else
                {
                    api.WithEnvironment(
                        EspadaConfigurationKeyConstants.BillingStripeSecretKey,
                        stripeApiKey);
                    worker.WithEnvironment(
                        EspadaConfigurationKeyConstants.BillingStripeSecretKey,
                        stripeApiKey);
                    stripe.WithListen(
                        api,
                        EspadaConfigurationValueConstants.StripeWebhookPath);
                    api.WithReference(
                        stripe,
                        EspadaConfigurationKeyConstants.BillingStripeWebhookSecret);
                    worker.WithReference(
                        stripe,
                        EspadaConfigurationKeyConstants.BillingStripeWebhookSecret);
                }

                string? embeddingBaseUrl =
                    builder.Configuration["EmbeddingGeneration:BaseUrl"];
                string? embeddingDefaultModel =
                    builder.Configuration["EmbeddingGeneration:DefaultModel"];
                if (!string.IsNullOrWhiteSpace(embeddingBaseUrl))
                {
                    worker.WithEnvironment(
                        EspadaConfigurationKeyConstants.EmbeddingBaseUrl,
                        embeddingBaseUrl);
                }

                if (!string.IsNullOrWhiteSpace(embeddingDefaultModel))
                {
                    api.WithEnvironment(
                        EspadaConfigurationKeyConstants.EmbeddingDefaultModel,
                        embeddingDefaultModel);
                    worker.WithEnvironment(
                        EspadaConfigurationKeyConstants.EmbeddingDefaultModel,
                        embeddingDefaultModel);
                }

                builder
                    .AddViteApp(EspadaResourceNameConstants.Web, "../Espada.Web")
                    .WithReference(api)
                    .WaitFor(api);
            }
        }

        private static string ResolveParameter(
            IDistributedApplicationBuilder builder,
            string parameterName,
            string fallback = "")
        {
            string? configuredParameterValue =
                builder.Configuration[
                    $"{EspadaConfigurationKeyConstants.ParametersSectionPrefix}{parameterName}"];

            return !string.IsNullOrWhiteSpace(configuredParameterValue)
                ? configuredParameterValue
                : fallback;
        }
    }
}