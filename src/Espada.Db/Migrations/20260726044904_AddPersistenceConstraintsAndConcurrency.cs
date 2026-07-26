using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Espada.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddPersistenceConstraintsAndConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Version",
                schema: "Espada",
                table: "Workspaces",
                type: "bigint",
                nullable: false,
                defaultValue: 1L);

            migrationBuilder.AddColumn<long>(
                name: "Version",
                schema: "Espada",
                table: "Sources",
                type: "bigint",
                nullable: false,
                defaultValue: 1L);

            migrationBuilder.AddColumn<long>(
                name: "Version",
                schema: "Espada",
                table: "ImportJobs",
                type: "bigint",
                nullable: false,
                defaultValue: 1L);

            migrationBuilder.AddColumn<long>(
                name: "Version",
                schema: "Espada",
                table: "ChunkBatches",
                type: "bigint",
                nullable: false,
                defaultValue: 1L);

            migrationBuilder.AddColumn<long>(
                name: "Version",
                schema: "Espada",
                table: "Artifacts",
                type: "bigint",
                nullable: false,
                defaultValue: 1L);

            migrationBuilder.CreateIndex(
                name: "UX_ChunkEmbeddings_ChunkId_ModelIdentifier_ModelVersion",
                schema: "Espada",
                table: "ChunkEmbeddings",
                columns: new[] { "ChunkId", "ModelIdentifier", "ModelVersion" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ArtifactRevisions_Artifacts_ArtifactId",
                schema: "Espada",
                table: "ArtifactRevisions",
                column: "ArtifactId",
                principalSchema: "Espada",
                principalTable: "Artifacts",
                principalColumn: "ArtifactId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Artifacts_Workspaces_WorkspaceId",
                schema: "Espada",
                table: "Artifacts",
                column: "WorkspaceId",
                principalSchema: "Espada",
                principalTable: "Workspaces",
                principalColumn: "WorkspaceId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChunkBatches_ArtifactRevisions_ArtifactRevisionId",
                schema: "Espada",
                table: "ChunkBatches",
                column: "ArtifactRevisionId",
                principalSchema: "Espada",
                principalTable: "ArtifactRevisions",
                principalColumn: "ArtifactRevisionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChunkEmbeddings_Chunks_ChunkId",
                schema: "Espada",
                table: "ChunkEmbeddings",
                column: "ChunkId",
                principalSchema: "Espada",
                principalTable: "Chunks",
                principalColumn: "ChunkId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chunks_ArtifactRevisions_ArtifactRevisionId",
                schema: "Espada",
                table: "Chunks",
                column: "ArtifactRevisionId",
                principalSchema: "Espada",
                principalTable: "ArtifactRevisions",
                principalColumn: "ArtifactRevisionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chunks_Artifacts_ArtifactId",
                schema: "Espada",
                table: "Chunks",
                column: "ArtifactId",
                principalSchema: "Espada",
                principalTable: "Artifacts",
                principalColumn: "ArtifactId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chunks_ChunkBatches_ChunkBatchId",
                schema: "Espada",
                table: "Chunks",
                column: "ChunkBatchId",
                principalSchema: "Espada",
                principalTable: "ChunkBatches",
                principalColumn: "ChunkBatchId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ImportJobs_Sources_SourceId",
                schema: "Espada",
                table: "ImportJobs",
                column: "SourceId",
                principalSchema: "Espada",
                principalTable: "Sources",
                principalColumn: "SourceId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ImportJobs_Workspaces_WorkspaceId",
                schema: "Espada",
                table: "ImportJobs",
                column: "WorkspaceId",
                principalSchema: "Espada",
                principalTable: "Workspaces",
                principalColumn: "WorkspaceId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sources_Workspaces_WorkspaceId",
                schema: "Espada",
                table: "Sources",
                column: "WorkspaceId",
                principalSchema: "Espada",
                principalTable: "Workspaces",
                principalColumn: "WorkspaceId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArtifactRevisions_Artifacts_ArtifactId",
                schema: "Espada",
                table: "ArtifactRevisions");

            migrationBuilder.DropForeignKey(
                name: "FK_Artifacts_Workspaces_WorkspaceId",
                schema: "Espada",
                table: "Artifacts");

            migrationBuilder.DropForeignKey(
                name: "FK_ChunkBatches_ArtifactRevisions_ArtifactRevisionId",
                schema: "Espada",
                table: "ChunkBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_ChunkEmbeddings_Chunks_ChunkId",
                schema: "Espada",
                table: "ChunkEmbeddings");

            migrationBuilder.DropForeignKey(
                name: "FK_Chunks_ArtifactRevisions_ArtifactRevisionId",
                schema: "Espada",
                table: "Chunks");

            migrationBuilder.DropForeignKey(
                name: "FK_Chunks_Artifacts_ArtifactId",
                schema: "Espada",
                table: "Chunks");

            migrationBuilder.DropForeignKey(
                name: "FK_Chunks_ChunkBatches_ChunkBatchId",
                schema: "Espada",
                table: "Chunks");

            migrationBuilder.DropForeignKey(
                name: "FK_ImportJobs_Sources_SourceId",
                schema: "Espada",
                table: "ImportJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_ImportJobs_Workspaces_WorkspaceId",
                schema: "Espada",
                table: "ImportJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_Sources_Workspaces_WorkspaceId",
                schema: "Espada",
                table: "Sources");

            migrationBuilder.DropIndex(
                name: "UX_ChunkEmbeddings_ChunkId_ModelIdentifier_ModelVersion",
                schema: "Espada",
                table: "ChunkEmbeddings");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "Espada",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "Espada",
                table: "Sources");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "Espada",
                table: "ImportJobs");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "Espada",
                table: "ChunkBatches");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "Espada",
                table: "Artifacts");
        }
    }
}
