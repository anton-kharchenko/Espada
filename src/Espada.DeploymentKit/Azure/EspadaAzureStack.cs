using Espada.DeploymentKit.Constants;
using Espada.DeploymentKit.Enums;
using Espada.DeploymentKit.Settings;
using Pulumi;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;
using Pulumi.AzureNative.ApplicationInsights;
using Pulumi.AzureNative.Authorization;
using Pulumi.AzureNative.ContainerRegistry;
using Pulumi.AzureNative.DBforPostgreSQL;
using Pulumi.AzureNative.DBforPostgreSQL.Inputs;
using Pulumi.AzureNative.KeyVault;
using Pulumi.AzureNative.KeyVault.Inputs;
using Pulumi.AzureNative.ManagedIdentity;
using Pulumi.AzureNative.OperationalInsights;
using Pulumi.AzureNative.OperationalInsights.Inputs;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using System.Security.Cryptography;
using System.Text;
using AppManagedIdentityArgs = Pulumi.AzureNative.App.Inputs.ManagedServiceIdentityArgs;
using Database = Pulumi.AzureNative.DBforPostgreSQL.Database;
using DatabaseArgs = Pulumi.AzureNative.DBforPostgreSQL.DatabaseArgs;
using PostgreSqlServer = Pulumi.AzureNative.DBforPostgreSQL.Server;
using PostgreSqlServerArgs = Pulumi.AzureNative.DBforPostgreSQL.ServerArgs;
using RandomPassword = Pulumi.Random.RandomPassword;
using RandomPasswordArgs = Pulumi.Random.RandomPasswordArgs;
using RegistrySkuArgs = Pulumi.AzureNative.ContainerRegistry.Inputs.SkuArgs;
using Secret = Pulumi.AzureNative.KeyVault.Secret;
using SecretArgs = Pulumi.AzureNative.KeyVault.SecretArgs;
using VaultSkuArgs = Pulumi.AzureNative.KeyVault.Inputs.SkuArgs;
using StorageAccount = Pulumi.AzureNative.Storage.StorageAccount;
using StorageAccountArgs = Pulumi.AzureNative.Storage.StorageAccountArgs;
using StorageSkuArgs = Pulumi.AzureNative.Storage.Inputs.SkuArgs;

namespace Espada.DeploymentKit.Azure;

internal static class EspadaAzureStack
{
    public static IDictionary<string, object?> Create(DeploymentSettings settings)
    {
        IDictionary<string, object?> websiteOutputs = settings.EnvironmentType == DeploymentEnvironmentType.Production
            ? WebsiteInfrastructure.Create(settings.Location)
            : new Dictionary<string, object?>();

        if (settings.TargetType == DeploymentTargetType.Website)
        {
            return websiteOutputs;
        }

        ResourceNames names = ResourceNames.Create(settings.EnvironmentType, settings.SubscriptionId);
        Dictionary<string, string> tags = new()
        {
            ["application"] = AzureDeploymentConstants.ApplicationName,
            ["environment"] = settings.EnvironmentName,
            ["managed-by"] = AzureDeploymentConstants.ManagedBy
        };

        ResourceGroup resourceGroup = new(names.ResourceGroup, new ResourceGroupArgs
        {
            ResourceGroupName = names.ResourceGroup,
            Location = settings.Location,
            Tags = tags
        });

        Registry registry = new(names.Registry, new RegistryArgs
        {
            RegistryName = names.Registry,
            ResourceGroupName = resourceGroup.Name,
            Location = settings.Location,
            AdminUserEnabled = false,
            Sku = new RegistrySkuArgs { Name = Pulumi.AzureNative.ContainerRegistry.SkuName.Standard },
            Tags = tags
        });

        Workspace workspace = new(names.LogAnalytics, new WorkspaceArgs
        {
            WorkspaceName = names.LogAnalytics,
            ResourceGroupName = resourceGroup.Name,
            Location = settings.Location,
            RetentionInDays = 30,
            Sku = new WorkspaceSkuArgs { Name = WorkspaceSkuNameEnum.PerGB2018 },
            Tags = tags
        });

        Component insights = new(names.ApplicationInsights, new ComponentArgs
        {
            ResourceName = names.ApplicationInsights,
            ResourceGroupName = resourceGroup.Name,
            Location = settings.Location,
            ApplicationType = ApplicationType.Web,
            Kind = "web",
            WorkspaceResourceId = workspace.Id,
            Tags = tags
        });

        Output<GetSharedKeysResult> workspaceKeys = GetSharedKeys.Invoke(new GetSharedKeysInvokeArgs
        {
            ResourceGroupName = resourceGroup.Name,
            WorkspaceName = workspace.Name
        });

        ManagedEnvironment containerEnvironment = new(names.ContainerEnvironment, new ManagedEnvironmentArgs
        {
            EnvironmentName = names.ContainerEnvironment,
            ResourceGroupName = resourceGroup.Name,
            Location = settings.Location,
            AppLogsConfiguration = new AppLogsConfigurationArgs
            {
                Destination = "log-analytics",
                LogAnalyticsConfiguration = new LogAnalyticsConfigurationArgs
                {
                    CustomerId = workspace.CustomerId,
                    SharedKey = Output.CreateSecret(workspaceKeys.Apply(keys => keys.PrimarySharedKey ?? string.Empty))
                }
            },
            Tags = tags
        });

        UserAssignedIdentity apiIdentity = CreateIdentity(names.ApiIdentity, settings.Location, resourceGroup, tags);
        UserAssignedIdentity workerIdentity = CreateIdentity(names.WorkerIdentity, settings.Location, resourceGroup, tags);
        UserAssignedIdentity migrationIdentity = CreateIdentity(names.MigrationIdentity, settings.Location, resourceGroup, tags);
        RoleAssignment apiRegistryPull = CreateRoleAssignment(
            $"{names.Api}-acr-pull",
            settings.SubscriptionId,
            registry.Id,
            apiIdentity.PrincipalId,
            AzureBuiltInRoleDefinitionIdConstants.AcrPull,
            apiIdentity);
        RoleAssignment workerRegistryPull = CreateRoleAssignment(
            $"{names.Worker}-acr-pull",
            settings.SubscriptionId,
            registry.Id,
            workerIdentity.PrincipalId,
            AzureBuiltInRoleDefinitionIdConstants.AcrPull,
            workerIdentity);
        RoleAssignment migrationRegistryPull = CreateRoleAssignment(
            $"{names.MigrationJob}-acr-pull",
            settings.SubscriptionId,
            registry.Id,
            migrationIdentity.PrincipalId,
            AzureBuiltInRoleDefinitionIdConstants.AcrPull,
            migrationIdentity);

        Vault vault = new(names.KeyVault, new VaultArgs
        {
            VaultName = names.KeyVault,
            ResourceGroupName = resourceGroup.Name,
            Location = settings.Location,
            Properties = new VaultPropertiesArgs
            {
                TenantId = settings.TenantId,
                AccessPolicies = [],
                EnablePurgeProtection = true,
                EnableRbacAuthorization = true,
                EnableSoftDelete = true,
                PublicNetworkAccess = AzureDeploymentConstants.Enabled,
                SoftDeleteRetentionInDays = 90,
                Sku = new VaultSkuArgs
                {
                    Family = "A",
                    Name = Pulumi.AzureNative.KeyVault.SkuName.Standard
                }
            },
            Tags = tags
        }, new CustomResourceOptions { RetainOnDelete = true });

        Output<GetClientConfigResult> client = GetClientConfig.Invoke();
        RoleAssignment deploymentVaultAdministrator = CreateRoleAssignment(
            $"{names.KeyVault}-deployment-admin",
            settings.SubscriptionId,
            vault.Id,
            client.Apply(value => value.ObjectId),
            AzureBuiltInRoleDefinitionIdConstants.KeyVaultAdministrator,
            vault);
        RoleAssignment apiVaultAccess = CreateRoleAssignment(
            $"{names.Api}-vault-secrets",
            settings.SubscriptionId,
            vault.Id,
            apiIdentity.PrincipalId,
            AzureBuiltInRoleDefinitionIdConstants.KeyVaultSecretsUser,
            apiIdentity);
        RoleAssignment workerVaultAccess = CreateRoleAssignment(
            $"{names.Worker}-vault-secrets",
            settings.SubscriptionId,
            vault.Id,
            workerIdentity.PrincipalId,
            AzureBuiltInRoleDefinitionIdConstants.KeyVaultSecretsUser,
            workerIdentity);
        RoleAssignment migrationVaultAccess = CreateRoleAssignment(
            $"{names.MigrationJob}-vault-secrets",
            settings.SubscriptionId,
            vault.Id,
            migrationIdentity.PrincipalId,
            AzureBuiltInRoleDefinitionIdConstants.KeyVaultSecretsUser,
            migrationIdentity);

        StorageAccount storage = new(names.StorageAccount, new StorageAccountArgs
        {
            AccountName = names.StorageAccount,
            ResourceGroupName = resourceGroup.Name,
            Location = settings.Location,
            Kind = Pulumi.AzureNative.Storage.Kind.StorageV2,
            Sku = new StorageSkuArgs { Name = Pulumi.AzureNative.Storage.SkuName.Standard_LRS },
            AllowBlobPublicAccess = false,
            AllowSharedKeyAccess = false,
            MinimumTlsVersion = MinimumTlsVersion.TLS1_2,
            PublicNetworkAccess = Pulumi.AzureNative.Storage.PublicNetworkAccess.Enabled,
            Tags = tags
        }, new CustomResourceOptions { RetainOnDelete = true });

        BlobContainer ingestionBlobs = new($"{names.StorageAccount}-ingestion", new BlobContainerArgs
        {
            AccountName = storage.Name,
            ResourceGroupName = resourceGroup.Name,
            ContainerName = "ingestion",
            PublicAccess = PublicAccess.None
        });
        RoleAssignment apiBlobAccess = CreateRoleAssignment(
            $"{names.Api}-blob-data",
            settings.SubscriptionId,
            storage.Id,
            apiIdentity.PrincipalId,
            AzureBuiltInRoleDefinitionIdConstants.StorageBlobDataContributor,
            apiIdentity);
        RoleAssignment workerBlobAccess = CreateRoleAssignment(
            $"{names.Worker}-blob-data",
            settings.SubscriptionId,
            storage.Id,
            workerIdentity.PrincipalId,
            AzureBuiltInRoleDefinitionIdConstants.StorageBlobDataContributor,
            workerIdentity);

        RandomPassword administratorPassword = CreatePassword("postgres-administrator-password");

        PostgreSqlServer postgres = new(names.PostgreSqlServer, new PostgreSqlServerArgs
        {
            ServerName = names.PostgreSqlServer,
            ResourceGroupName = resourceGroup.Name,
            Location = settings.Location,
            Version = AzureDeploymentConstants.PostgreSqlVersion,
            AdministratorLogin = AzureDeploymentConstants.PostgreSqlAdministratorLogin,
            AdministratorLoginPassword = administratorPassword.Result,
            AuthConfig = new AuthConfigArgs
            {
                ActiveDirectoryAuth = AzureDeploymentConstants.Disabled,
                PasswordAuth = AzureDeploymentConstants.Enabled
            },
            Backup = new Pulumi.AzureNative.DBforPostgreSQL.Inputs.BackupArgs
            {
                BackupRetentionDays = settings.EnvironmentType == DeploymentEnvironmentType.Production ? 14 : 7,
                GeoRedundantBackup = GeographicallyRedundantBackup.Disabled
            },
            Network = new NetworkArgs
            {
                PublicNetworkAccess = ServerPublicNetworkAccessState.Enabled
            },
            Sku = new Pulumi.AzureNative.DBforPostgreSQL.Inputs.SkuArgs
            {
                Name = settings.EnvironmentType == DeploymentEnvironmentType.Production
                    ? "Standard_D2ds_v5"
                    : "Standard_B1ms",
                Tier = settings.EnvironmentType == DeploymentEnvironmentType.Production
                    ? SkuTier.GeneralPurpose
                    : SkuTier.Burstable
            },
            Storage = new StorageArgs
            {
                StorageSizeGB = settings.EnvironmentType == DeploymentEnvironmentType.Production ? 128 : 32
            },
            Tags = tags
        }, new CustomResourceOptions { RetainOnDelete = true });

        Database database = new(names.PostgreSqlDatabase, new DatabaseArgs
        {
            DatabaseName = names.PostgreSqlDatabase,
            ResourceGroupName = resourceGroup.Name,
            ServerName = postgres.Name,
            Charset = "UTF8"
        });

        FirewallRule azureServicesFirewall = new(
            $"{names.PostgreSqlServer}-azure-services",
            new FirewallRuleArgs
            {
                FirewallRuleName = "AllowAzureServices",
                ResourceGroupName = resourceGroup.Name,
                ServerName = postgres.Name,
                StartIpAddress = "0.0.0.0",
                EndIpAddress = "0.0.0.0"
            });

        Output<string> administratorConnectionString = Output.Tuple(
            postgres.FullyQualifiedDomainName,
            administratorPassword.Result).Apply(values =>
                CreatePostgreSqlConnectionString(
                    values.Item1,
                    names.PostgreSqlDatabase,
                    AzureDeploymentConstants.PostgreSqlAdministratorLogin,
                    values.Item2));

        Secret apiKey = CreateVaultSecret(
            AzureDeploymentConstants.ApiKeySecret,
            Output.CreateSecret(settings.ApiKey!),
            vault,
            resourceGroup,
            tags,
            deploymentVaultAdministrator);
        CreateVaultSecret(
            AzureDeploymentConstants.AdministratorPasswordSecret,
            administratorPassword.Result,
            vault,
            resourceGroup,
            tags,
            deploymentVaultAdministrator);
        Secret administratorConnectionStringSecret = CreateVaultSecret(
            AzureDeploymentConstants.AdministratorConnectionStringSecret,
            administratorConnectionString,
            vault,
            resourceGroup,
            tags,
            deploymentVaultAdministrator);

        if (!settings.DeployWorkloads)
        {
            return MergeOutputs(CreateOutputs(resourceGroup, registry, null, null), websiteOutputs);
        }

        Output<string> apiImage = registry.LoginServer.Apply(
            loginServer => $"{loginServer}/{AzureDeploymentConstants.ApiImageRepository}:{settings.ImageTag}");
        Output<string> databaseImage = registry.LoginServer.Apply(
            loginServer => $"{loginServer}/{AzureDeploymentConstants.DatabaseImageRepository}:{settings.ImageTag}");
        Output<string> workerImage = registry.LoginServer.Apply(
            loginServer => $"{loginServer}/{AzureDeploymentConstants.WorkerImageRepository}:{settings.ImageTag}");
        Output<string> blobContainerUri = Output.Tuple(storage.Name, ingestionBlobs.Name)
            .Apply(values => $"https://{values.Item1}.blob.core.windows.net/{values.Item2}");

        Job migrationJob = new(names.MigrationJob, new JobArgs
        {
            JobName = names.MigrationJob,
            ResourceGroupName = resourceGroup.Name,
            Location = settings.Location,
            EnvironmentId = containerEnvironment.Id,
            Identity = UserAssignedIdentity(migrationIdentity.Id),
            Configuration = new JobConfigurationArgs
            {
                TriggerType = Pulumi.AzureNative.App.TriggerType.Manual,
                ReplicaRetryLimit = 0,
                ReplicaTimeout = 900,
                ManualTriggerConfig = SingleReplicaManualTrigger(),
                Registries = [RegistryCredentials(registry, migrationIdentity)],
                Secrets =
                [
                    KeyVaultSecretReference(
                        AzureDeploymentConstants.AdministratorConnectionStringSecret,
                        administratorConnectionStringSecret,
                        migrationIdentity)
                ]
            },
            Template = new JobTemplateArgs
            {
                Containers =
                [
                    new ContainerArgs
                    {
                        Name = "migrations",
                        Image = databaseImage,
                        Args = ["migrate"],
                        Env =
                        [
                            SecretEnvironmentVariable(
                                DatabaseConfigurationNames.ConnectionStringEnvironmentVariable,
                                AzureDeploymentConstants.AdministratorConnectionStringSecret)
                        ],
                        Resources = DatabaseJobResources()
                    }
                ]
            },
            Tags = tags
        }, new CustomResourceOptions
        {
            DependsOn =
            [
                database,
                azureServicesFirewall,
                administratorConnectionStringSecret,
                migrationRegistryPull,
                migrationVaultAccess
            ]
        });

        ContainerApp api = new(names.Api, new ContainerAppArgs
        {
            ContainerAppName = names.Api,
            ResourceGroupName = resourceGroup.Name,
            Location = settings.Location,
            EnvironmentId = containerEnvironment.Id,
            Identity = UserAssignedIdentity(apiIdentity.Id),
            Configuration = new Pulumi.AzureNative.App.Inputs.ConfigurationArgs
            {
                ActiveRevisionsMode = ActiveRevisionsMode.Single,
                Ingress = new IngressArgs
                {
                    External = true,
                    AllowInsecure = false,
                    TargetPort = 8080,
                    Transport = IngressTransportMethod.Auto
                },
                Registries = [RegistryCredentials(registry, apiIdentity)],
                Secrets =
                [
                    KeyVaultSecretReference(AzureDeploymentConstants.ApiKeySecret, apiKey, apiIdentity),
                    KeyVaultSecretReference(
                        AzureDeploymentConstants.AdministratorConnectionStringSecret,
                        administratorConnectionStringSecret,
                        apiIdentity)
                ]
            },
            Template = new TemplateArgs
            {
                Containers =
                [
                    new ContainerArgs
                    {
                        Name = "api",
                        Image = apiImage,
                        Env =
                        [
                            new EnvironmentVarArgs
                            {
                                Name = AzureDeploymentConstants.AspNetCoreHttpPortsEnvironmentVariable,
                                Value = "8080"
                            },
                            SecretEnvironmentVariable(
                                ApiConfigurationNames.ConnectionStringEnvironmentVariable,
                                AzureDeploymentConstants.AdministratorConnectionStringSecret),
                            SecretEnvironmentVariable(
                                ApiConfigurationNames.ApiKeyEnvironmentVariable,
                                AzureDeploymentConstants.ApiKeySecret),
                            new EnvironmentVarArgs
                            {
                                Name = AzureDeploymentConstants.ApplicationInsightsConnectionStringEnvironmentVariable,
                                Value = insights.ConnectionString
                            },
                            new EnvironmentVarArgs
                            {
                                Name = AzureDeploymentConstants.BlobProviderEnvironmentVariable,
                                Value = "Azure"
                            },
                            new EnvironmentVarArgs
                            {
                                Name = AzureDeploymentConstants.BlobContainerUriEnvironmentVariable,
                                Value = blobContainerUri
                            }
                        ],
                        Resources = new ContainerResourcesArgs
                        {
                            Cpu = 0.5,
                            Memory = "1Gi"
                        }
                    }
                ],
                Scale = new ScaleArgs
                {
                    MinReplicas = settings.ApiEnabled ? 1 : 0,
                    MaxReplicas = settings.EnvironmentType == DeploymentEnvironmentType.Production ? 3 : 1
                }
            },
            Tags = tags
        }, new CustomResourceOptions
        {
            DependsOn =
            [
                apiRegistryPull,
                apiVaultAccess,
                administratorConnectionStringSecret,
                apiBlobAccess,
                ingestionBlobs
            ]
        });

        ContainerApp worker = new(names.Worker, new ContainerAppArgs
        {
            ContainerAppName = names.Worker,
            ResourceGroupName = resourceGroup.Name,
            Location = settings.Location,
            EnvironmentId = containerEnvironment.Id,
            Identity = UserAssignedIdentity(workerIdentity.Id),
            Configuration = new Pulumi.AzureNative.App.Inputs.ConfigurationArgs
            {
                ActiveRevisionsMode = ActiveRevisionsMode.Single,
                Registries = [RegistryCredentials(registry, workerIdentity)],
                Secrets =
                [
                    KeyVaultSecretReference(
                        AzureDeploymentConstants.AdministratorConnectionStringSecret,
                        administratorConnectionStringSecret,
                        workerIdentity)
                ]
            },
            Template = new TemplateArgs
            {
                Containers =
                [
                    new ContainerArgs
                    {
                        Name = "worker",
                        Image = workerImage,
                        Env =
                        [
                            SecretEnvironmentVariable(
                                DatabaseConfigurationNames.ConnectionStringEnvironmentVariable,
                                AzureDeploymentConstants.AdministratorConnectionStringSecret),
                            new EnvironmentVarArgs
                            {
                                Name = AzureDeploymentConstants.ApplicationInsightsConnectionStringEnvironmentVariable,
                                Value = insights.ConnectionString
                            },
                            new EnvironmentVarArgs
                            {
                                Name = AzureDeploymentConstants.BlobProviderEnvironmentVariable,
                                Value = "Azure"
                            },
                            new EnvironmentVarArgs
                            {
                                Name = AzureDeploymentConstants.BlobContainerUriEnvironmentVariable,
                                Value = blobContainerUri
                            }
                        ],
                        Resources = new ContainerResourcesArgs
                        {
                            Cpu = 0.5,
                            Memory = "1Gi"
                        }
                    }
                ],
                Scale = new ScaleArgs
                {
                    MinReplicas = 1,
                    MaxReplicas = settings.EnvironmentType == DeploymentEnvironmentType.Production ? 3 : 1
                }
            },
            Tags = tags
        }, new CustomResourceOptions
        {
            DependsOn =
            [
                workerRegistryPull,
                workerVaultAccess,
                workerBlobAccess,
                ingestionBlobs,
                administratorConnectionStringSecret
            ]
        });

        return MergeOutputs(CreateOutputs(resourceGroup, registry, migrationJob, api, worker), websiteOutputs);
    }

    private static IDictionary<string, object?> MergeOutputs(
        IDictionary<string, object?> applicationOutputs,
        IDictionary<string, object?> websiteOutputs)
    {
        foreach ((string key, object? value) in websiteOutputs)
        {
            applicationOutputs.TryAdd(key, value);
        }

        return applicationOutputs;
    }

    private static RandomPassword CreatePassword(string name) =>
        new(name, new RandomPasswordArgs
        {
            Length = AzureDeploymentConstants.PostgreSqlPasswordLength,
            Lower = true,
            MinLower = 2,
            Numeric = true,
            MinNumeric = 2,
            Special = false,
            Upper = true,
            MinUpper = 2
        });

    private static Secret CreateVaultSecret(
        string name,
        Input<string> value,
        Vault vault,
        ResourceGroup resourceGroup,
        Dictionary<string, string> tags,
        RoleAssignment deploymentVaultAdministrator) =>
        new(name, new SecretArgs
        {
            SecretName = name,
            VaultName = vault.Name,
            ResourceGroupName = resourceGroup.Name,
            Properties = new SecretPropertiesArgs { Value = value },
            Tags = tags
        }, new CustomResourceOptions
        {
            DependsOn = [deploymentVaultAdministrator]
        });

    private static Pulumi.AzureNative.App.Inputs.SecretArgs KeyVaultSecretReference(
        string name,
        Secret secret,
        UserAssignedIdentity identity) =>
        new()
        {
            Name = name,
            KeyVaultUrl = secret.Properties.Apply(value => value.SecretUriWithVersion),
            Identity = identity.Id
        };

    private static EnvironmentVarArgs SecretEnvironmentVariable(string name, string secretName) =>
        new()
        {
            Name = name,
            SecretRef = secretName
        };

    private static RegistryCredentialsArgs RegistryCredentials(
        Registry registry,
        UserAssignedIdentity identity) =>
        new()
        {
            Server = registry.LoginServer,
            Identity = identity.Id
        };

    private static JobConfigurationManualTriggerConfigArgs SingleReplicaManualTrigger() =>
        new()
        {
            Parallelism = 1,
            ReplicaCompletionCount = 1
        };

    private static ContainerResourcesArgs DatabaseJobResources() =>
        new()
        {
            Cpu = 0.5,
            Memory = "1Gi"
        };

    private static string CreatePostgreSqlConnectionString(
        string host,
        string database,
        string username,
        string password) =>
        $"Host={host};Port=5432;Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=false";

    private static IDictionary<string, object?> CreateOutputs(
        ResourceGroup resourceGroup,
        Registry registry,
        Job? migrationJob,
        ContainerApp? api,
        ContainerApp? worker = null) =>
        new Dictionary<string, object?>
        {
            [AzureDeploymentConstants.ResourceGroupOutput] = resourceGroup.Name,
            [AzureDeploymentConstants.RegistryNameOutput] = registry.Name,
            [AzureDeploymentConstants.RegistryLoginServerOutput] = registry.LoginServer,
            [AzureDeploymentConstants.MigrationJobOutput] = migrationJob is null ? string.Empty : migrationJob.Name,
            [AzureDeploymentConstants.ApiUrlOutput] = api is null
                ? string.Empty
                : api.Configuration.Apply(
                    configuration => $"https://{configuration?.Ingress?.Fqdn ?? string.Empty}"),
            [AzureDeploymentConstants.WorkerOutput] = worker?.Name ?? Output.Create(string.Empty)
        };

    private static UserAssignedIdentity CreateIdentity(
        string name,
        string location,
        ResourceGroup resourceGroup,
        Dictionary<string, string> tags) =>
        new(name, new Pulumi.AzureNative.ManagedIdentity.UserAssignedIdentityArgs
        {
            ResourceName = name,
            ResourceGroupName = resourceGroup.Name,
            Location = location,
            Tags = tags
        });

    private static AppManagedIdentityArgs UserAssignedIdentity(Input<string> identityId) =>
        new()
        {
            Type = "UserAssigned",
            UserAssignedIdentities = [identityId]
        };

    private static RoleAssignment CreateRoleAssignment(
        string seed,
        string subscriptionId,
        Input<string> scope,
        Input<string> principalId,
        string roleId,
        Pulumi.Resource dependency) =>
        new(CreateDeterministicGuid(seed), new RoleAssignmentArgs
        {
            PrincipalId = principalId,
            PrincipalType = Pulumi.AzureNative.Authorization.PrincipalType.ServicePrincipal,
            RoleDefinitionId = $"/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/{roleId}",
            Scope = scope
        }, new CustomResourceOptions
        {
            DependsOn = [dependency]
        });

    private static string CreateDeterministicGuid(string value)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return new Guid(hash.AsSpan(0, 16)).ToString();
    }
}