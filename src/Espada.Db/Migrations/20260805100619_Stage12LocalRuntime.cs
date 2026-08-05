using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Espada.Db.Migrations
{
    /// <summary>
    /// Adds Stage 12 agent and sync state and permits local-only projects.
    /// Recovery: stop all Espada processes and restore the pre-migration database backup before rolling back.
    /// Down converts null remote URIs to empty strings, so export local-only project identity before rollback.
    /// </summary>
    public partial class Stage12LocalRuntime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CanonicalRemoteUri",
                schema: "Espada",
                table: "Projects",
                type: "varchar(2048)",
                maxLength: 2048,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(2048)",
                oldMaxLength: 2048);

            migrationBuilder.CreateTable(
                name: "AgentApprovalStatusTypes",
                schema: "Espada",
                columns: table => new
                {
                    AgentApprovalStatusTypeId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentApprovalStatusTypes", x => x.AgentApprovalStatusTypeId);
                });

            migrationBuilder.CreateTable(
                name: "AgentSessionEventTypes",
                schema: "Espada",
                columns: table => new
                {
                    AgentSessionEventTypeId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentSessionEventTypes", x => x.AgentSessionEventTypeId);
                });

            migrationBuilder.CreateTable(
                name: "AgentSessionStatusTypes",
                schema: "Espada",
                columns: table => new
                {
                    AgentSessionStatusTypeId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentSessionStatusTypes", x => x.AgentSessionStatusTypeId);
                });

            migrationBuilder.CreateTable(
                name: "AgentVendorTypes",
                schema: "Espada",
                columns: table => new
                {
                    AgentVendorTypeId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentVendorTypes", x => x.AgentVendorTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                schema: "Espada",
                columns: table => new
                {
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastSeenAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.DeviceId);
                });

            migrationBuilder.CreateTable(
                name: "SyncConflictStatusTypes",
                schema: "Espada",
                columns: table => new
                {
                    SyncConflictStatusTypeId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncConflictStatusTypes", x => x.SyncConflictStatusTypeId);
                });

            migrationBuilder.CreateTable(
                name: "AgentProfiles",
                schema: "Espada",
                columns: table => new
                {
                    AgentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorTypeId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SettingsJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentProfiles", x => x.AgentProfileId);
                    table.ForeignKey(
                        name: "FK_AgentProfiles_AgentVendorTypes_VendorTypeId",
                        column: x => x.VendorTypeId,
                        principalSchema: "Espada",
                        principalTable: "AgentVendorTypes",
                        principalColumn: "AgentVendorTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgentProfiles_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalSchema: "Espada",
                        principalTable: "Workspaces",
                        principalColumn: "WorkspaceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AgentInstallations",
                schema: "Espada",
                columns: table => new
                {
                    AgentInstallationId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorTypeId = table.Column<int>(type: "integer", nullable: false),
                    ExecutablePath = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    DetectedVersion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsAuthenticated = table.Column<bool>(type: "boolean", nullable: false),
                    DetectedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentInstallations", x => x.AgentInstallationId);
                    table.ForeignKey(
                        name: "FK_AgentInstallations_AgentVendorTypes_VendorTypeId",
                        column: x => x.VendorTypeId,
                        principalSchema: "Espada",
                        principalTable: "AgentVendorTypes",
                        principalColumn: "AgentVendorTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgentInstallations_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalSchema: "Espada",
                        principalTable: "Devices",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SyncCursors",
                schema: "Espada",
                columns: table => new
                {
                    SyncCursorId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServerCursor = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncCursors", x => x.SyncCursorId);
                    table.ForeignKey(
                        name: "FK_SyncCursors_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalSchema: "Espada",
                        principalTable: "Devices",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SyncCursors_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalSchema: "Espada",
                        principalTable: "Workspaces",
                        principalColumn: "WorkspaceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SyncEvents",
                schema: "Espada",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<long>(type: "bigint", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Operation = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BaseVersion = table.Column<long>(type: "bigint", nullable: true),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PayloadType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    PayloadHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncEvents", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_SyncEvents_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalSchema: "Espada",
                        principalTable: "Devices",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SyncEvents_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalSchema: "Espada",
                        principalTable: "Workspaces",
                        principalColumn: "WorkspaceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AgentSessions",
                schema: "Espada",
                columns: table => new
                {
                    AgentSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Prompt = table.Column<string>(type: "text", nullable: false),
                    BranchName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    WorktreePath = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FinishedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentSessions", x => x.AgentSessionId);
                    table.ForeignKey(
                        name: "FK_AgentSessions_AgentProfiles_AgentProfileId",
                        column: x => x.AgentProfileId,
                        principalSchema: "Espada",
                        principalTable: "AgentProfiles",
                        principalColumn: "AgentProfileId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgentSessions_AgentSessionStatusTypes_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Espada",
                        principalTable: "AgentSessionStatusTypes",
                        principalColumn: "AgentSessionStatusTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgentSessions_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalSchema: "Espada",
                        principalTable: "Devices",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgentSessions_Projects_ProjectId_WorkspaceId",
                        columns: x => new { x.ProjectId, x.WorkspaceId },
                        principalSchema: "Espada",
                        principalTable: "Projects",
                        principalColumns: new[] { "ProjectId", "WorkspaceId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgentSessions_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalSchema: "Espada",
                        principalTable: "Workspaces",
                        principalColumn: "WorkspaceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SyncConflicts",
                schema: "Espada",
                columns: table => new
                {
                    SyncConflictId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocalEventId = table.Column<Guid>(type: "uuid", nullable: false),
                    RemoteEventId = table.Column<Guid>(type: "uuid", nullable: false),
                    DetailsJson = table.Column<string>(type: "jsonb", nullable: false),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResolvedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncConflicts", x => x.SyncConflictId);
                    table.ForeignKey(
                        name: "FK_SyncConflicts_SyncConflictStatusTypes_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Espada",
                        principalTable: "SyncConflictStatusTypes",
                        principalColumn: "SyncConflictStatusTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SyncConflicts_SyncEvents_LocalEventId",
                        column: x => x.LocalEventId,
                        principalSchema: "Espada",
                        principalTable: "SyncEvents",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SyncConflicts_SyncEvents_RemoteEventId",
                        column: x => x.RemoteEventId,
                        principalSchema: "Espada",
                        principalTable: "SyncEvents",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SyncConflicts_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalSchema: "Espada",
                        principalTable: "Workspaces",
                        principalColumn: "WorkspaceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AgentSessionEvents",
                schema: "Espada",
                columns: table => new
                {
                    AgentSessionEventId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<long>(type: "bigint", nullable: false),
                    TypeId = table.Column<int>(type: "integer", nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentSessionEvents", x => x.AgentSessionEventId);
                    table.ForeignKey(
                        name: "FK_AgentSessionEvents_AgentSessionEventTypes_TypeId",
                        column: x => x.TypeId,
                        principalSchema: "Espada",
                        principalTable: "AgentSessionEventTypes",
                        principalColumn: "AgentSessionEventTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgentSessionEvents_AgentSessions_AgentSessionId",
                        column: x => x.AgentSessionId,
                        principalSchema: "Espada",
                        principalTable: "AgentSessions",
                        principalColumn: "AgentSessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AgentApprovals",
                schema: "Espada",
                columns: table => new
                {
                    AgentApprovalId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestEventId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToolName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ArgumentsJson = table.Column<string>(type: "jsonb", nullable: false),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    RequestedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DecidedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentApprovals", x => x.AgentApprovalId);
                    table.ForeignKey(
                        name: "FK_AgentApprovals_AgentApprovalStatusTypes_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Espada",
                        principalTable: "AgentApprovalStatusTypes",
                        principalColumn: "AgentApprovalStatusTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgentApprovals_AgentSessionEvents_RequestEventId",
                        column: x => x.RequestEventId,
                        principalSchema: "Espada",
                        principalTable: "AgentSessionEvents",
                        principalColumn: "AgentSessionEventId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgentApprovals_AgentSessions_AgentSessionId",
                        column: x => x.AgentSessionId,
                        principalSchema: "Espada",
                        principalTable: "AgentSessions",
                        principalColumn: "AgentSessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "Espada",
                table: "AgentApprovalStatusTypes",
                columns: new[] { "AgentApprovalStatusTypeId", "Name" },
                values: new object[,]
                {
                    { 1, "Pending" },
                    { 2, "Approved" },
                    { 3, "Denied" }
                });

            migrationBuilder.InsertData(
                schema: "Espada",
                table: "AgentSessionEventTypes",
                columns: new[] { "AgentSessionEventTypeId", "Name" },
                values: new object[,]
                {
                    { 1, "AssistantOutput" },
                    { 2, "ToolRequest" },
                    { 3, "ToolResult" },
                    { 4, "ApprovalRequest" },
                    { 5, "Status" },
                    { 6, "Usage" },
                    { 7, "Error" },
                    { 8, "DiffUpdate" }
                });

            migrationBuilder.InsertData(
                schema: "Espada",
                table: "AgentSessionStatusTypes",
                columns: new[] { "AgentSessionStatusTypeId", "Name" },
                values: new object[,]
                {
                    { 1, "Created" },
                    { 2, "Running" },
                    { 3, "WaitingForApproval" },
                    { 4, "Completed" },
                    { 5, "Failed" },
                    { 6, "Cancelled" }
                });

            migrationBuilder.InsertData(
                schema: "Espada",
                table: "AgentVendorTypes",
                columns: new[] { "AgentVendorTypeId", "Name" },
                values: new object[,]
                {
                    { 1, "Codex" },
                    { 2, "Claude" },
                    { 3, "Gemini" },
                    { 4, "Grok" }
                });

            migrationBuilder.InsertData(
                schema: "Espada",
                table: "SourceTypes",
                columns: new[] { "SourceTypeId", "Name" },
                values: new object[] { 6, "Repository" });

            migrationBuilder.InsertData(
                schema: "Espada",
                table: "SyncConflictStatusTypes",
                columns: new[] { "SyncConflictStatusTypeId", "Name" },
                values: new object[,]
                {
                    { 1, "Open" },
                    { 2, "Resolved" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentApprovals_AgentSessionId",
                schema: "Espada",
                table: "AgentApprovals",
                column: "AgentSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentApprovals_StatusId",
                schema: "Espada",
                table: "AgentApprovals",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "UX_AgentApprovals_RequestEventId",
                schema: "Espada",
                table: "AgentApprovals",
                column: "RequestEventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_AgentApprovalStatusTypes_Name",
                schema: "Espada",
                table: "AgentApprovalStatusTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgentInstallations_VendorTypeId",
                schema: "Espada",
                table: "AgentInstallations",
                column: "VendorTypeId");

            migrationBuilder.CreateIndex(
                name: "UX_AgentInstallations_DeviceId_Vendor_ExecutablePath",
                schema: "Espada",
                table: "AgentInstallations",
                columns: new[] { "DeviceId", "VendorTypeId", "ExecutablePath" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgentProfiles_VendorTypeId",
                schema: "Espada",
                table: "AgentProfiles",
                column: "VendorTypeId");

            migrationBuilder.CreateIndex(
                name: "UX_AgentProfiles_WorkspaceId_Vendor_Name",
                schema: "Espada",
                table: "AgentProfiles",
                columns: new[] { "WorkspaceId", "VendorTypeId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgentSessionEvents_TypeId",
                schema: "Espada",
                table: "AgentSessionEvents",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "UX_AgentSessionEvents_AgentSessionId_Sequence",
                schema: "Espada",
                table: "AgentSessionEvents",
                columns: new[] { "AgentSessionId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_AgentSessionEventTypes_Name",
                schema: "Espada",
                table: "AgentSessionEventTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgentSessions_AgentProfileId",
                schema: "Espada",
                table: "AgentSessions",
                column: "AgentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentSessions_DeviceId",
                schema: "Espada",
                table: "AgentSessions",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentSessions_ProjectId_WorkspaceId",
                schema: "Espada",
                table: "AgentSessions",
                columns: new[] { "ProjectId", "WorkspaceId" });

            migrationBuilder.CreateIndex(
                name: "IX_AgentSessions_StatusId",
                schema: "Espada",
                table: "AgentSessions",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentSessions_WorkspaceId_CreatedAtUtc",
                schema: "Espada",
                table: "AgentSessions",
                columns: new[] { "WorkspaceId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "UX_AgentSessionStatusTypes_Name",
                schema: "Espada",
                table: "AgentSessionStatusTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_AgentVendorTypes_Name",
                schema: "Espada",
                table: "AgentVendorTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyncConflicts_RemoteEventId",
                schema: "Espada",
                table: "SyncConflicts",
                column: "RemoteEventId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncConflicts_StatusId",
                schema: "Espada",
                table: "SyncConflicts",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncConflicts_WorkspaceId",
                schema: "Espada",
                table: "SyncConflicts",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "UX_SyncConflicts_LocalEventId_RemoteEventId",
                schema: "Espada",
                table: "SyncConflicts",
                columns: new[] { "LocalEventId", "RemoteEventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_SyncConflictStatusTypes_Name",
                schema: "Espada",
                table: "SyncConflictStatusTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyncCursors_WorkspaceId",
                schema: "Espada",
                table: "SyncCursors",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "UX_SyncCursors_DeviceId_WorkspaceId",
                schema: "Espada",
                table: "SyncCursors",
                columns: new[] { "DeviceId", "WorkspaceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyncEvents_WorkspaceId_EntityType_EntityId",
                schema: "Espada",
                table: "SyncEvents",
                columns: new[] { "WorkspaceId", "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "UX_SyncEvents_DeviceId_Sequence",
                schema: "Espada",
                table: "SyncEvents",
                columns: new[] { "DeviceId", "Sequence" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentApprovals",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "AgentInstallations",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "SyncConflicts",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "SyncCursors",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "AgentApprovalStatusTypes",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "AgentSessionEvents",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "SyncConflictStatusTypes",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "SyncEvents",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "AgentSessionEventTypes",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "AgentSessions",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "AgentProfiles",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "AgentSessionStatusTypes",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "Devices",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "AgentVendorTypes",
                schema: "Espada");

            migrationBuilder.DeleteData(
                schema: "Espada",
                table: "SourceTypes",
                keyColumn: "SourceTypeId",
                keyValue: 6);

            migrationBuilder.AlterColumn<string>(
                name: "CanonicalRemoteUri",
                schema: "Espada",
                table: "Projects",
                type: "varchar(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(2048)",
                oldMaxLength: 2048,
                oldNullable: true);
        }
    }
}
