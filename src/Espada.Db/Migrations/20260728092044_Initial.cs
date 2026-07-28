using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;
using System;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Espada.Db.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Espada");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "ArtifactStatusTypes",
                schema: "Espada",
                columns: table => new
                {
                    ArtifactStatusTypeId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtifactStatusTypes", x => x.ArtifactStatusTypeId);
                });

            migrationBuilder.CreateTable(
                name: "ArtifactTypes",
                schema: "Espada",
                columns: table => new
                {
                    ArtifactTypeId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtifactTypes", x => x.ArtifactTypeId);
                });

            migrationBuilder.CreateTable(
                name: "ChunkBatchStatusTypes",
                schema: "Espada",
                columns: table => new
                {
                    ChunkBatchStatusTypeId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChunkBatchStatusTypes", x => x.ChunkBatchStatusTypeId);
                });

            migrationBuilder.CreateTable(
                name: "ChunkingStrategyTypes",
                schema: "Espada",
                columns: table => new
                {
                    ChunkingStrategyTypeId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChunkingStrategyTypes", x => x.ChunkingStrategyTypeId);
                });

            migrationBuilder.CreateTable(
                name: "ImportStatusTypes",
                schema: "Espada",
                columns: table => new
                {
                    ImportStatusTypeId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportStatusTypes", x => x.ImportStatusTypeId);
                });

            migrationBuilder.CreateTable(
                name: "OneTimeBootstrapCodes",
                schema: "Espada",
                columns: table => new
                {
                    OneTimeBootstrapCodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodeHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Purpose = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    IdentityIssuer = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IdentitySubject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ConsumedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OneTimeBootstrapCodes", x => x.OneTimeBootstrapCodeId);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictApplications",
                schema: "Espada",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ClientId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ClientSecret = table.Column<string>(type: "text", nullable: true),
                    ClientType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConcurrencyToken = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConsentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    DisplayNames = table.Column<string>(type: "text", nullable: true),
                    JsonWebKeySet = table.Column<string>(type: "text", nullable: true),
                    Permissions = table.Column<string>(type: "text", nullable: true),
                    PostLogoutRedirectUris = table.Column<string>(type: "text", nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    RedirectUris = table.Column<string>(type: "text", nullable: true),
                    Requirements = table.Column<string>(type: "text", nullable: true),
                    Settings = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictApplications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictScopes",
                schema: "Espada",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConcurrencyToken = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Descriptions = table.Column<string>(type: "text", nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    DisplayNames = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    Resources = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictScopes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                schema: "Espada",
                columns: table => new
                {
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.OrganizationId);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "Espada",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EventVersion = table.Column<int>(type: "integer", nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AvailableAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Attempt = table.Column<int>(type: "integer", nullable: false),
                    LeaseOwner = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LeaseExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ProcessedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SanitizedError = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.EventId);
                });

            migrationBuilder.CreateTable(
                name: "PaymentEvents",
                schema: "Espada",
                columns: table => new
                {
                    ProviderEventId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EventType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ApiVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    ProviderCreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReceivedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AvailableAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Attempt = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    LeaseOwner = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LeaseExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ProcessedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SanitizedError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentEvents", x => x.ProviderEventId);
                });

            migrationBuilder.CreateTable(
                name: "SourceStatusTypes",
                schema: "Espada",
                columns: table => new
                {
                    SourceStatusTypeId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceStatusTypes", x => x.SourceStatusTypeId);
                });

            migrationBuilder.CreateTable(
                name: "SourceTypes",
                schema: "Espada",
                columns: table => new
                {
                    SourceTypeId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceTypes", x => x.SourceTypeId);
                });

            migrationBuilder.CreateTable(
                name: "WorkspaceStatusTypes",
                schema: "Espada",
                columns: table => new
                {
                    WorkspaceStatusTypeId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkspaceStatusTypes", x => x.WorkspaceStatusTypeId);
                });

            migrationBuilder.CreateTable(
                name: "WorkspaceTypes",
                schema: "Espada",
                columns: table => new
                {
                    WorkspaceTypeId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkspaceTypes", x => x.WorkspaceTypeId);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictAuthorizations",
                schema: "Espada",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConcurrencyToken = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    Scopes = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Subject = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictAuthorizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpenIddictAuthorizations_OpenIddictApplications_Application~",
                        column: x => x.ApplicationId,
                        principalSchema: "Espada",
                        principalTable: "OpenIddictApplications",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OrganizationMemberships",
                schema: "Espada",
                columns: table => new
                {
                    OrganizationMembershipId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Issuer = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    Subject = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    JoinedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationMemberships", x => x.OrganizationMembershipId);
                    table.ForeignKey(
                        name: "FK_OrganizationMemberships_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "Espada",
                        principalTable: "Organizations",
                        principalColumn: "OrganizationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Workspaces",
                schema: "Espada",
                columns: table => new
                {
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    TypeId = table.Column<int>(type: "integer", nullable: false),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    ArchivedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workspaces", x => x.WorkspaceId);
                    table.ForeignKey(
                        name: "FK_Workspaces_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "Espada",
                        principalTable: "Organizations",
                        principalColumn: "OrganizationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Workspaces_WorkspaceStatusTypes_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Espada",
                        principalTable: "WorkspaceStatusTypes",
                        principalColumn: "WorkspaceStatusTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Workspaces_WorkspaceTypes_TypeId",
                        column: x => x.TypeId,
                        principalSchema: "Espada",
                        principalTable: "WorkspaceTypes",
                        principalColumn: "WorkspaceTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictTokens",
                schema: "Espada",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    AuthorizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConcurrencyToken = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Payload = table.Column<string>(type: "text", nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    RedemptionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReferenceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Subject = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    Type = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpenIddictTokens_OpenIddictApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "Espada",
                        principalTable: "OpenIddictApplications",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OpenIddictTokens_OpenIddictAuthorizations_AuthorizationId",
                        column: x => x.AuthorizationId,
                        principalSchema: "Espada",
                        principalTable: "OpenIddictAuthorizations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Artifacts",
                schema: "Espada",
                columns: table => new
                {
                    ArtifactId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Kind = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    TypeId = table.Column<int>(type: "integer", nullable: false),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CurrentRevisionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CurrentRevisionNumber = table.Column<int>(type: "integer", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    ArchivedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artifacts", x => x.ArtifactId);
                    table.UniqueConstraint("AK_Artifacts_ArtifactId_WorkspaceId", x => new { x.ArtifactId, x.WorkspaceId });
                    table.ForeignKey(
                        name: "FK_Artifacts_ArtifactStatusTypes_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Espada",
                        principalTable: "ArtifactStatusTypes",
                        principalColumn: "ArtifactStatusTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Artifacts_ArtifactTypes_TypeId",
                        column: x => x.TypeId,
                        principalSchema: "Espada",
                        principalTable: "ArtifactTypes",
                        principalColumn: "ArtifactTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Artifacts_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalSchema: "Espada",
                        principalTable: "Workspaces",
                        principalColumn: "WorkspaceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BillingCustomers",
                schema: "Espada",
                columns: table => new
                {
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderCustomerId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ProviderSubscriptionId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Plan = table.Column<int>(type: "integer", nullable: false),
                    SubscriptionStatus = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PaymentFailedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastProviderEventAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingCustomers", x => x.WorkspaceId);
                    table.ForeignKey(
                        name: "FK_BillingCustomers_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalSchema: "Espada",
                        principalTable: "Workspaces",
                        principalColumn: "WorkspaceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                schema: "Espada",
                columns: table => new
                {
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    CanonicalRemoteUri = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: false),
                    LocalAliases = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.ProjectId);
                    table.UniqueConstraint("AK_Projects_ProjectId_WorkspaceId", x => new { x.ProjectId, x.WorkspaceId });
                    table.ForeignKey(
                        name: "FK_Projects_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalSchema: "Espada",
                        principalTable: "Workspaces",
                        principalColumn: "WorkspaceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sources",
                schema: "Espada",
                columns: table => new
                {
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    TypeId = table.Column<int>(type: "integer", nullable: false),
                    Locator = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: false),
                    DefinitionJson = table.Column<string>(type: "jsonb", nullable: true),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    ArchivedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sources", x => x.SourceId);
                    table.ForeignKey(
                        name: "FK_Sources_SourceStatusTypes_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Espada",
                        principalTable: "SourceStatusTypes",
                        principalColumn: "SourceStatusTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sources_SourceTypes_TypeId",
                        column: x => x.TypeId,
                        principalSchema: "Espada",
                        principalTable: "SourceTypes",
                        principalColumn: "SourceTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sources_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalSchema: "Espada",
                        principalTable: "Workspaces",
                        principalColumn: "WorkspaceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsageLedgerEntries",
                schema: "Espada",
                columns: table => new
                {
                    EntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Metric = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Quantity = table.Column<long>(type: "bigint", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsageLedgerEntries", x => x.EntryId);
                    table.ForeignKey(
                        name: "FK_UsageLedgerEntries_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalSchema: "Espada",
                        principalTable: "Workspaces",
                        principalColumn: "WorkspaceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkspaceMemberships",
                schema: "Espada",
                columns: table => new
                {
                    WorkspaceMembershipId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Issuer = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    JoinedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkspaceMemberships", x => x.WorkspaceMembershipId);
                    table.ForeignKey(
                        name: "FK_WorkspaceMemberships_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalSchema: "Espada",
                        principalTable: "Workspaces",
                        principalColumn: "WorkspaceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ArtifactRevisions",
                schema: "Espada",
                columns: table => new
                {
                    ArtifactRevisionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArtifactId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    RevisionNumber = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtifactRevisions", x => x.ArtifactRevisionId);
                    table.UniqueConstraint("AK_ArtifactRevisions_ArtifactRevisionId_ArtifactId_Kind", x => new { x.ArtifactRevisionId, x.ArtifactId, x.Kind });
                    table.UniqueConstraint("AK_ArtifactRevisions_ArtifactRevisionId_Kind", x => new { x.ArtifactRevisionId, x.Kind });
                    table.UniqueConstraint("AK_ArtifactRevisions_ArtifactRevisionId_WorkspaceId", x => new { x.ArtifactRevisionId, x.WorkspaceId });
                    table.ForeignKey(
                        name: "FK_ArtifactRevisions_Artifacts_ArtifactId_WorkspaceId",
                        columns: x => new { x.ArtifactId, x.WorkspaceId },
                        principalSchema: "Espada",
                        principalTable: "Artifacts",
                        principalColumns: new[] { "ArtifactId", "WorkspaceId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                schema: "Espada",
                columns: table => new
                {
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    ArchivedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.TaskId);
                    table.UniqueConstraint("AK_Tasks_TaskId_ProjectId_WorkspaceId", x => new { x.TaskId, x.ProjectId, x.WorkspaceId });
                    table.ForeignKey(
                        name: "FK_Tasks_Projects_ProjectId_WorkspaceId",
                        columns: x => new { x.ProjectId, x.WorkspaceId },
                        principalSchema: "Espada",
                        principalTable: "Projects",
                        principalColumns: new[] { "ProjectId", "WorkspaceId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tasks_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalSchema: "Espada",
                        principalTable: "Workspaces",
                        principalColumn: "WorkspaceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ImportJobs",
                schema: "Espada",
                columns: table => new
                {
                    ImportJobId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    Stage = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    IdempotencyKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    RequestFingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    OptionsJson = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    RequestedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    ArtifactId = table.Column<Guid>(type: "uuid", nullable: true),
                    ArtifactRevisionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChunkBatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    RawBlobHash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ParsedBlobHash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FailureCode = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    FailureReason = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportJobs", x => x.ImportJobId);
                    table.ForeignKey(
                        name: "FK_ImportJobs_ImportStatusTypes_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Espada",
                        principalTable: "ImportStatusTypes",
                        principalColumn: "ImportStatusTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ImportJobs_Sources_SourceId",
                        column: x => x.SourceId,
                        principalSchema: "Espada",
                        principalTable: "Sources",
                        principalColumn: "SourceId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ImportJobs_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalSchema: "Espada",
                        principalTable: "Workspaces",
                        principalColumn: "WorkspaceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsageReconciliationOutbox",
                schema: "Espada",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    LedgerEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    AvailableAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Attempt = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    LeaseOwner = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LeaseExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ProcessedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SanitizedError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsageReconciliationOutbox", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_UsageReconciliationOutbox_UsageLedgerEntries_LedgerEntryId",
                        column: x => x.LedgerEntryId,
                        principalSchema: "Espada",
                        principalTable: "UsageLedgerEntries",
                        principalColumn: "EntryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChunkBatches",
                schema: "Espada",
                columns: table => new
                {
                    ChunkBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArtifactId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArtifactRevisionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StrategyId = table.Column<int>(type: "integer", nullable: false),
                    StrategyVersion = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    RequestedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    ChunkCount = table.Column<int>(type: "integer", nullable: true),
                    FailureReason = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChunkBatches", x => x.ChunkBatchId);
                    table.ForeignKey(
                        name: "FK_ChunkBatches_ArtifactRevisions_ArtifactRevisionId",
                        column: x => x.ArtifactRevisionId,
                        principalSchema: "Espada",
                        principalTable: "ArtifactRevisions",
                        principalColumn: "ArtifactRevisionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChunkBatches_ChunkBatchStatusTypes_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Espada",
                        principalTable: "ChunkBatchStatusTypes",
                        principalColumn: "ChunkBatchStatusTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChunkBatches_ChunkingStrategyTypes_StrategyId",
                        column: x => x.StrategyId,
                        principalSchema: "Espada",
                        principalTable: "ChunkingStrategyTypes",
                        principalColumn: "ChunkingStrategyTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InstructionRules",
                schema: "Espada",
                columns: table => new
                {
                    ArtifactRevisionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleKey = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Kind = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstructionRules_ArtifactRevisionId_RuleKey", x => new { x.ArtifactRevisionId, x.RuleKey });
                    table.CheckConstraint("CK_InstructionRules_Kind", "\"Kind\" = 'instruction'");
                    table.ForeignKey(
                        name: "FK_InstructionRules_ArtifactRevisions_ArtifactRevisionId_Kind",
                        columns: x => new { x.ArtifactRevisionId, x.Kind },
                        principalSchema: "Espada",
                        principalTable: "ArtifactRevisions",
                        principalColumns: new[] { "ArtifactRevisionId", "Kind" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MemoryMetadata",
                schema: "Espada",
                columns: table => new
                {
                    MemoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArtifactId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArtifactRevisionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    Category = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    Confidence = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    UserConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    ClientIdentity = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    SessionIdentity = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    CapturedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    SupersededMemoryId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemoryMetadata", x => x.MemoryId);
                    table.CheckConstraint("CK_MemoryMetadata_Kind", "\"Kind\" = 'memory'");
                    table.ForeignKey(
                        name: "FK_MemoryMetadata_ArtifactRevisions_ArtifactRevisionId_Artifac~",
                        columns: x => new { x.ArtifactRevisionId, x.ArtifactId, x.Kind },
                        principalSchema: "Espada",
                        principalTable: "ArtifactRevisions",
                        principalColumns: new[] { "ArtifactRevisionId", "ArtifactId", "Kind" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemoryMetadata_MemoryMetadata_SupersededMemoryId",
                        column: x => x.SupersededMemoryId,
                        principalSchema: "Espada",
                        principalTable: "MemoryMetadata",
                        principalColumn: "MemoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PolicyRules",
                schema: "Espada",
                columns: table => new
                {
                    ArtifactRevisionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleKey = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Kind = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Enforcement = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyRules_ArtifactRevisionId_RuleKey", x => new { x.ArtifactRevisionId, x.RuleKey });
                    table.CheckConstraint("CK_PolicyRules_Kind", "\"Kind\" = 'policy'");
                    table.ForeignKey(
                        name: "FK_PolicyRules_ArtifactRevisions_ArtifactRevisionId_Kind",
                        columns: x => new { x.ArtifactRevisionId, x.Kind },
                        principalSchema: "Espada",
                        principalTable: "ArtifactRevisions",
                        principalColumns: new[] { "ArtifactRevisionId", "Kind" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bindings",
                schema: "Espada",
                columns: table => new
                {
                    BindingId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArtifactRevisionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    RepositoryCanonicalUri = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true),
                    RepositoryRelativePathPrefix = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    Branch = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: true),
                    Agent = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bindings", x => x.BindingId);
                    table.CheckConstraint("CK_Bindings_TaskRequiresProject", "\"TaskId\" IS NULL OR \"ProjectId\" IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_Bindings_ArtifactRevisions_ArtifactRevisionId_WorkspaceId",
                        columns: x => new { x.ArtifactRevisionId, x.WorkspaceId },
                        principalSchema: "Espada",
                        principalTable: "ArtifactRevisions",
                        principalColumns: new[] { "ArtifactRevisionId", "WorkspaceId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bindings_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "Espada",
                        principalTable: "Organizations",
                        principalColumn: "OrganizationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bindings_Projects_ProjectId_WorkspaceId",
                        columns: x => new { x.ProjectId, x.WorkspaceId },
                        principalSchema: "Espada",
                        principalTable: "Projects",
                        principalColumns: new[] { "ProjectId", "WorkspaceId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bindings_Tasks_TaskId_ProjectId_WorkspaceId",
                        columns: x => new { x.TaskId, x.ProjectId, x.WorkspaceId },
                        principalSchema: "Espada",
                        principalTable: "Tasks",
                        principalColumns: new[] { "TaskId", "ProjectId", "WorkspaceId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bindings_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalSchema: "Espada",
                        principalTable: "Workspaces",
                        principalColumn: "WorkspaceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IngestionJobs",
                schema: "Espada",
                columns: table => new
                {
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportJobId = table.Column<Guid>(type: "uuid", nullable: false),
                    Stage = table.Column<int>(type: "integer", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Attempt = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AvailableAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LeaseOwner = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LeaseExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FailureCategory = table.Column<int>(type: "integer", nullable: true),
                    SanitizedError = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngestionJobs", x => x.JobId);
                    table.ForeignKey(
                        name: "FK_IngestionJobs_ImportJobs_ImportJobId",
                        column: x => x.ImportJobId,
                        principalSchema: "Espada",
                        principalTable: "ImportJobs",
                        principalColumn: "ImportJobId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chunks",
                schema: "Espada",
                columns: table => new
                {
                    ChunkId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChunkBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArtifactId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArtifactRevisionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChunkNumber = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    SourceStart = table.Column<int>(type: "integer", nullable: true),
                    SourceLength = table.Column<int>(type: "integer", nullable: true),
                    StrategyId = table.Column<int>(type: "integer", nullable: false),
                    StrategyVersion = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chunks", x => x.ChunkId);
                    table.ForeignKey(
                        name: "FK_Chunks_ArtifactRevisions_ArtifactRevisionId",
                        column: x => x.ArtifactRevisionId,
                        principalSchema: "Espada",
                        principalTable: "ArtifactRevisions",
                        principalColumn: "ArtifactRevisionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Chunks_Artifacts_ArtifactId",
                        column: x => x.ArtifactId,
                        principalSchema: "Espada",
                        principalTable: "Artifacts",
                        principalColumn: "ArtifactId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Chunks_ChunkBatches_ChunkBatchId",
                        column: x => x.ChunkBatchId,
                        principalSchema: "Espada",
                        principalTable: "ChunkBatches",
                        principalColumn: "ChunkBatchId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Chunks_ChunkingStrategyTypes_StrategyId",
                        column: x => x.StrategyId,
                        principalSchema: "Espada",
                        principalTable: "ChunkingStrategyTypes",
                        principalColumn: "ChunkingStrategyTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChunkEmbeddings",
                schema: "Espada",
                columns: table => new
                {
                    ChunkEmbeddingId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChunkId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChunkContentHash = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    ModelIdentifier = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    ModelVersion = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Dimensions = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChunkEmbeddings", x => x.ChunkEmbeddingId);
                    table.ForeignKey(
                        name: "FK_ChunkEmbeddings_Chunks_ChunkId",
                        column: x => x.ChunkId,
                        principalSchema: "Espada",
                        principalTable: "Chunks",
                        principalColumn: "ChunkId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChunkEmbeddingVectors",
                schema: "Espada",
                columns: table => new
                {
                    ChunkEmbeddingId = table.Column<Guid>(type: "uuid", nullable: false),
                    Vector = table.Column<Vector>(type: "vector", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChunkEmbeddingVectors", x => x.ChunkEmbeddingId);
                    table.ForeignKey(
                        name: "FK_ChunkEmbeddingVectors_ChunkEmbeddings_ChunkEmbeddingId",
                        column: x => x.ChunkEmbeddingId,
                        principalSchema: "Espada",
                        principalTable: "ChunkEmbeddings",
                        principalColumn: "ChunkEmbeddingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "Espada",
                table: "ArtifactStatusTypes",
                columns: new[] { "ArtifactStatusTypeId", "Name" },
                values: new object[,]
                {
                    { 1, "Active" },
                    { 2, "Archived" }
                });

            migrationBuilder.InsertData(
                schema: "Espada",
                table: "ArtifactTypes",
                columns: new[] { "ArtifactTypeId", "Name" },
                values: new object[,]
                {
                    { 1, "Text" },
                    { 2, "Markdown" },
                    { 3, "File" },
                    { 4, "WebPage" },
                    { 5, "Conversation" }
                });

            migrationBuilder.InsertData(
                schema: "Espada",
                table: "ChunkBatchStatusTypes",
                columns: new[] { "ChunkBatchStatusTypeId", "Name" },
                values: new object[,]
                {
                    { 1, "Requested" },
                    { 2, "Running" },
                    { 3, "Succeeded" },
                    { 4, "Failed" }
                });

            migrationBuilder.InsertData(
                schema: "Espada",
                table: "ChunkingStrategyTypes",
                columns: new[] { "ChunkingStrategyTypeId", "Name" },
                values: new object[,]
                {
                    { 1, "FixedSize" },
                    { 2, "Recursive" },
                    { 3, "Markdown" },
                    { 4, "Semantic" },
                    { 5, "Code" },
                    { 6, "Custom" }
                });

            migrationBuilder.InsertData(
                schema: "Espada",
                table: "ImportStatusTypes",
                columns: new[] { "ImportStatusTypeId", "Name" },
                values: new object[,]
                {
                    { 1, "Requested" },
                    { 2, "Running" },
                    { 3, "Succeeded" },
                    { 4, "Failed" },
                    { 5, "Cancelled" }
                });

            migrationBuilder.InsertData(
                schema: "Espada",
                table: "SourceStatusTypes",
                columns: new[] { "SourceStatusTypeId", "Name" },
                values: new object[,]
                {
                    { 1, "Active" },
                    { 2, "Archived" }
                });

            migrationBuilder.InsertData(
                schema: "Espada",
                table: "SourceTypes",
                columns: new[] { "SourceTypeId", "Name" },
                values: new object[,]
                {
                    { 1, "File" },
                    { 2, "WebPage" },
                    { 3, "PlainText" },
                    { 4, "Conversation" },
                    { 5, "Connector" }
                });

            migrationBuilder.InsertData(
                schema: "Espada",
                table: "WorkspaceStatusTypes",
                columns: new[] { "WorkspaceStatusTypeId", "Name" },
                values: new object[,]
                {
                    { 1, "Active" },
                    { 2, "Archived" }
                });

            migrationBuilder.InsertData(
                schema: "Espada",
                table: "WorkspaceTypes",
                columns: new[] { "WorkspaceTypeId", "Name" },
                values: new object[,]
                {
                    { 1, "Personal" },
                    { 2, "Team" },
                    { 3, "Organization" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArtifactRevisions_ArtifactId",
                schema: "Espada",
                table: "ArtifactRevisions",
                column: "ArtifactId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtifactRevisions_ArtifactId_WorkspaceId",
                schema: "Espada",
                table: "ArtifactRevisions",
                columns: new[] { "ArtifactId", "WorkspaceId" });

            migrationBuilder.CreateIndex(
                name: "UX_ArtifactRevisions_ArtifactId_RevisionNumber",
                schema: "Espada",
                table: "ArtifactRevisions",
                columns: new[] { "ArtifactId", "RevisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Artifacts_StatusId",
                schema: "Espada",
                table: "Artifacts",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Artifacts_TypeId",
                schema: "Espada",
                table: "Artifacts",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Artifacts_WorkspaceId",
                schema: "Espada",
                table: "Artifacts",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Artifacts_WorkspaceId_Title",
                schema: "Espada",
                table: "Artifacts",
                columns: new[] { "WorkspaceId", "Title" });

            migrationBuilder.CreateIndex(
                name: "UX_ArtifactStatusTypes_Name",
                schema: "Espada",
                table: "ArtifactStatusTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_ArtifactTypes_Name",
                schema: "Espada",
                table: "ArtifactTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BillingCustomers_ProviderCustomerId",
                schema: "Espada",
                table: "BillingCustomers",
                column: "ProviderCustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bindings_ArtifactRevisionId_WorkspaceId",
                schema: "Espada",
                table: "Bindings",
                columns: new[] { "ArtifactRevisionId", "WorkspaceId" });

            migrationBuilder.CreateIndex(
                name: "IX_Bindings_OrganizationId",
                schema: "Espada",
                table: "Bindings",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Bindings_ProjectId_WorkspaceId",
                schema: "Espada",
                table: "Bindings",
                columns: new[] { "ProjectId", "WorkspaceId" });

            migrationBuilder.CreateIndex(
                name: "IX_Bindings_TaskId_ProjectId_WorkspaceId",
                schema: "Espada",
                table: "Bindings",
                columns: new[] { "TaskId", "ProjectId", "WorkspaceId" });

            migrationBuilder.CreateIndex(
                name: "IX_Bindings_WorkspaceId_ArtifactRevisionId",
                schema: "Espada",
                table: "Bindings",
                columns: new[] { "WorkspaceId", "ArtifactRevisionId" });

            migrationBuilder.CreateIndex(
                name: "IX_ChunkBatches_ArtifactId",
                schema: "Espada",
                table: "ChunkBatches",
                column: "ArtifactId");

            migrationBuilder.CreateIndex(
                name: "IX_ChunkBatches_ArtifactRevisionId",
                schema: "Espada",
                table: "ChunkBatches",
                column: "ArtifactRevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChunkBatches_Revision_Strategy_Version",
                schema: "Espada",
                table: "ChunkBatches",
                columns: new[] { "ArtifactRevisionId", "StrategyId", "StrategyVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_ChunkBatches_StatusId",
                schema: "Espada",
                table: "ChunkBatches",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ChunkBatches_StrategyId",
                schema: "Espada",
                table: "ChunkBatches",
                column: "StrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_ChunkBatches_WorkspaceId",
                schema: "Espada",
                table: "ChunkBatches",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "UX_ChunkBatchStatusTypes_Name",
                schema: "Espada",
                table: "ChunkBatchStatusTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChunkEmbeddings_ChunkId",
                schema: "Espada",
                table: "ChunkEmbeddings",
                column: "ChunkId");

            migrationBuilder.CreateIndex(
                name: "IX_ChunkEmbeddings_WorkspaceId",
                schema: "Espada",
                table: "ChunkEmbeddings",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "UX_ChunkEmbeddings_ChunkId_ModelIdentifier_ModelVersion",
                schema: "Espada",
                table: "ChunkEmbeddings",
                columns: new[] { "ChunkId", "ModelIdentifier", "ModelVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_ChunkingStrategyTypes_Name",
                schema: "Espada",
                table: "ChunkingStrategyTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chunks_ArtifactId",
                schema: "Espada",
                table: "Chunks",
                column: "ArtifactId");

            migrationBuilder.CreateIndex(
                name: "IX_Chunks_ArtifactRevisionId",
                schema: "Espada",
                table: "Chunks",
                column: "ArtifactRevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Chunks_ChunkBatchId",
                schema: "Espada",
                table: "Chunks",
                column: "ChunkBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Chunks_StrategyId",
                schema: "Espada",
                table: "Chunks",
                column: "StrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_Chunks_WorkspaceId",
                schema: "Espada",
                table: "Chunks",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "UX_Chunks_ChunkBatchId_ChunkNumber",
                schema: "Espada",
                table: "Chunks",
                columns: new[] { "ChunkBatchId", "ChunkNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImportJobs_SourceId",
                schema: "Espada",
                table: "ImportJobs",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportJobs_StatusId",
                schema: "Espada",
                table: "ImportJobs",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportJobs_WorkspaceId",
                schema: "Espada",
                table: "ImportJobs",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "UX_ImportJobs_WorkspaceId_IdempotencyKey",
                schema: "Espada",
                table: "ImportJobs",
                columns: new[] { "WorkspaceId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_ImportStatusTypes_Name",
                schema: "Espada",
                table: "ImportStatusTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IngestionJobs_IdempotencyKey",
                schema: "Espada",
                table: "IngestionJobs",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IngestionJobs_ImportJobId",
                schema: "Espada",
                table: "IngestionJobs",
                column: "ImportJobId");

            migrationBuilder.CreateIndex(
                name: "IX_IngestionJobs_Status_AvailableAtUtc_LeaseExpiresAtUtc",
                schema: "Espada",
                table: "IngestionJobs",
                columns: new[] { "Status", "AvailableAtUtc", "LeaseExpiresAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_InstructionRules_ArtifactRevisionId_Kind",
                schema: "Espada",
                table: "InstructionRules",
                columns: new[] { "ArtifactRevisionId", "Kind" });

            migrationBuilder.CreateIndex(
                name: "IX_MemoryMetadata_ArtifactRevisionId_ArtifactId_Kind",
                schema: "Espada",
                table: "MemoryMetadata",
                columns: new[] { "ArtifactRevisionId", "ArtifactId", "Kind" });

            migrationBuilder.CreateIndex(
                name: "UX_MemoryMetadata_ArtifactId_ArtifactRevisionId",
                schema: "Espada",
                table: "MemoryMetadata",
                columns: new[] { "ArtifactId", "ArtifactRevisionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_MemoryMetadata_SupersededMemoryId",
                schema: "Espada",
                table: "MemoryMetadata",
                column: "SupersededMemoryId",
                unique: true,
                filter: "\"SupersededMemoryId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_OneTimeBootstrapCodes_CodeHash",
                schema: "Espada",
                table: "OneTimeBootstrapCodes",
                column: "CodeHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictApplications_ClientId",
                schema: "Espada",
                table: "OpenIddictApplications",
                column: "ClientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictAuthorizations_ApplicationId_Status_Subject_Type",
                schema: "Espada",
                table: "OpenIddictAuthorizations",
                columns: new[] { "ApplicationId", "Status", "Subject", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictScopes_Name",
                schema: "Espada",
                table: "OpenIddictScopes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictTokens_ApplicationId_Status_Subject_Type",
                schema: "Espada",
                table: "OpenIddictTokens",
                columns: new[] { "ApplicationId", "Status", "Subject", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictTokens_AuthorizationId",
                schema: "Espada",
                table: "OpenIddictTokens",
                column: "AuthorizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictTokens_ReferenceId",
                schema: "Espada",
                table: "OpenIddictTokens",
                column: "ReferenceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_OrganizationMemberships_OrganizationId_Issuer_Subject",
                schema: "Espada",
                table: "OrganizationMemberships",
                columns: new[] { "OrganizationId", "Issuer", "Subject" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Name",
                schema: "Espada",
                table: "Organizations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedAtUtc_AvailableAtUtc",
                schema: "Espada",
                table: "OutboxMessages",
                columns: new[] { "ProcessedAtUtc", "AvailableAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentEvents_Status_AvailableAtUtc_LeaseExpiresAtUtc",
                schema: "Espada",
                table: "PaymentEvents",
                columns: new[] { "Status", "AvailableAtUtc", "LeaseExpiresAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PolicyRules_ArtifactRevisionId_Kind",
                schema: "Espada",
                table: "PolicyRules",
                columns: new[] { "ArtifactRevisionId", "Kind" });

            migrationBuilder.CreateIndex(
                name: "UX_Projects_WorkspaceId_CanonicalRemoteUri",
                schema: "Espada",
                table: "Projects",
                columns: new[] { "WorkspaceId", "CanonicalRemoteUri" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sources_StatusId",
                schema: "Espada",
                table: "Sources",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Sources_TypeId",
                schema: "Espada",
                table: "Sources",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Sources_WorkspaceId",
                schema: "Espada",
                table: "Sources",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "UX_Sources_WorkspaceId_Locator",
                schema: "Espada",
                table: "Sources",
                columns: new[] { "WorkspaceId", "Locator" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_SourceStatusTypes_Name",
                schema: "Espada",
                table: "SourceStatusTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_SourceTypes_Name",
                schema: "Espada",
                table: "SourceTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ProjectId_WorkspaceId",
                schema: "Espada",
                table: "Tasks",
                columns: new[] { "ProjectId", "WorkspaceId" });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_WorkspaceId_ProjectId_Status",
                schema: "Espada",
                table: "Tasks",
                columns: new[] { "WorkspaceId", "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_UsageLedgerEntries_WorkspaceId_Metric_IdempotencyKey",
                schema: "Espada",
                table: "UsageLedgerEntries",
                columns: new[] { "WorkspaceId", "Metric", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsageReconciliationOutbox_LedgerEntryId",
                schema: "Espada",
                table: "UsageReconciliationOutbox",
                column: "LedgerEntryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsageReconciliationOutbox_Status_AvailableAtUtc_LeaseExpire~",
                schema: "Espada",
                table: "UsageReconciliationOutbox",
                columns: new[] { "Status", "AvailableAtUtc", "LeaseExpiresAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceMemberships_WorkspaceId_Issuer_Subject",
                schema: "Espada",
                table: "WorkspaceMemberships",
                columns: new[] { "WorkspaceId", "Issuer", "Subject" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_Name",
                schema: "Espada",
                table: "Workspaces",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_OrganizationId",
                schema: "Espada",
                table: "Workspaces",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_StatusId",
                schema: "Espada",
                table: "Workspaces",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_TypeId",
                schema: "Espada",
                table: "Workspaces",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "UX_WorkspaceStatusTypes_Name",
                schema: "Espada",
                table: "WorkspaceStatusTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_WorkspaceTypes_Name",
                schema: "Espada",
                table: "WorkspaceTypes",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillingCustomers",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "Bindings",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "ChunkEmbeddingVectors",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "IngestionJobs",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "InstructionRules",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "MemoryMetadata",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "OneTimeBootstrapCodes",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "OpenIddictScopes",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "OpenIddictTokens",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "OrganizationMemberships",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "PaymentEvents",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "PolicyRules",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "UsageReconciliationOutbox",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "WorkspaceMemberships",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "Tasks",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "ChunkEmbeddings",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "ImportJobs",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "OpenIddictAuthorizations",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "UsageLedgerEntries",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "Projects",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "Chunks",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "ImportStatusTypes",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "Sources",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "OpenIddictApplications",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "ChunkBatches",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "SourceStatusTypes",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "SourceTypes",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "ArtifactRevisions",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "ChunkBatchStatusTypes",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "ChunkingStrategyTypes",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "Artifacts",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "ArtifactStatusTypes",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "ArtifactTypes",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "Workspaces",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "Organizations",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "WorkspaceStatusTypes",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "WorkspaceTypes",
                schema: "Espada");
        }
    }
}