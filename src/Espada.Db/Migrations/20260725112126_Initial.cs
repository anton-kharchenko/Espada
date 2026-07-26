using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

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

            migrationBuilder.CreateTable(
                name: "ArtifactRevisions",
                schema: "Espada",
                columns: table => new
                {
                    ArtifactRevisionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArtifactId = table.Column<Guid>(type: "uuid", nullable: false),
                    RevisionNumber = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtifactRevisions", x => x.ArtifactRevisionId);
                });

            migrationBuilder.CreateTable(
                name: "Artifacts",
                schema: "Espada",
                columns: table => new
                {
                    ArtifactId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    TypeId = table.Column<int>(type: "integer", nullable: false),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CurrentRevisionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CurrentRevisionNumber = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    ArchivedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artifacts", x => x.ArtifactId);
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
                    FailureReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChunkBatches", x => x.ChunkBatchId);
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
                    RequestedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    ArtifactId = table.Column<Guid>(type: "uuid", nullable: true),
                    ArtifactRevisionId = table.Column<Guid>(type: "uuid", nullable: true),
                    FailureCode = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    FailureReason = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportJobs", x => x.ImportJobId);
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
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    ArchivedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sources", x => x.SourceId);
                });

            migrationBuilder.CreateTable(
                name: "Workspaces",
                schema: "Espada",
                columns: table => new
                {
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    TypeId = table.Column<int>(type: "integer", nullable: false),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    ArchivedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workspaces", x => x.WorkspaceId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArtifactRevisions_ArtifactId",
                schema: "Espada",
                table: "ArtifactRevisions",
                column: "ArtifactId");

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
                name: "IX_ChunkBatches_WorkspaceId",
                schema: "Espada",
                table: "ChunkBatches",
                column: "WorkspaceId");

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
                name: "IX_Sources_StatusId",
                schema: "Espada",
                table: "Sources",
                column: "StatusId");

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
                name: "IX_Workspaces_Name",
                schema: "Espada",
                table: "Workspaces",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_StatusId",
                schema: "Espada",
                table: "Workspaces",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArtifactRevisions",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "Artifacts",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "ChunkBatches",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "ChunkEmbeddings",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "Chunks",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "ImportJobs",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "Sources",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "Workspaces",
                schema: "Espada");
        }
    }
}
