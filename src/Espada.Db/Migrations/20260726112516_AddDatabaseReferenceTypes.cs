using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Espada.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddDatabaseReferenceTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "IX_Workspaces_TypeId",
                schema: "Espada",
                table: "Workspaces",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Sources_TypeId",
                schema: "Espada",
                table: "Sources",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Chunks_StrategyId",
                schema: "Espada",
                table: "Chunks",
                column: "StrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_ChunkBatches_StrategyId",
                schema: "Espada",
                table: "ChunkBatches",
                column: "StrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_Artifacts_TypeId",
                schema: "Espada",
                table: "Artifacts",
                column: "TypeId");

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
                name: "UX_ChunkBatchStatusTypes_Name",
                schema: "Espada",
                table: "ChunkBatchStatusTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_ChunkingStrategyTypes_Name",
                schema: "Espada",
                table: "ChunkingStrategyTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_ImportStatusTypes_Name",
                schema: "Espada",
                table: "ImportStatusTypes",
                column: "Name",
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

            migrationBuilder.AddForeignKey(
                name: "FK_Artifacts_ArtifactStatusTypes_StatusId",
                schema: "Espada",
                table: "Artifacts",
                column: "StatusId",
                principalSchema: "Espada",
                principalTable: "ArtifactStatusTypes",
                principalColumn: "ArtifactStatusTypeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Artifacts_ArtifactTypes_TypeId",
                schema: "Espada",
                table: "Artifacts",
                column: "TypeId",
                principalSchema: "Espada",
                principalTable: "ArtifactTypes",
                principalColumn: "ArtifactTypeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChunkBatches_ChunkBatchStatusTypes_StatusId",
                schema: "Espada",
                table: "ChunkBatches",
                column: "StatusId",
                principalSchema: "Espada",
                principalTable: "ChunkBatchStatusTypes",
                principalColumn: "ChunkBatchStatusTypeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChunkBatches_ChunkingStrategyTypes_StrategyId",
                schema: "Espada",
                table: "ChunkBatches",
                column: "StrategyId",
                principalSchema: "Espada",
                principalTable: "ChunkingStrategyTypes",
                principalColumn: "ChunkingStrategyTypeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chunks_ChunkingStrategyTypes_StrategyId",
                schema: "Espada",
                table: "Chunks",
                column: "StrategyId",
                principalSchema: "Espada",
                principalTable: "ChunkingStrategyTypes",
                principalColumn: "ChunkingStrategyTypeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ImportJobs_ImportStatusTypes_StatusId",
                schema: "Espada",
                table: "ImportJobs",
                column: "StatusId",
                principalSchema: "Espada",
                principalTable: "ImportStatusTypes",
                principalColumn: "ImportStatusTypeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sources_SourceStatusTypes_StatusId",
                schema: "Espada",
                table: "Sources",
                column: "StatusId",
                principalSchema: "Espada",
                principalTable: "SourceStatusTypes",
                principalColumn: "SourceStatusTypeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sources_SourceTypes_TypeId",
                schema: "Espada",
                table: "Sources",
                column: "TypeId",
                principalSchema: "Espada",
                principalTable: "SourceTypes",
                principalColumn: "SourceTypeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Workspaces_WorkspaceStatusTypes_StatusId",
                schema: "Espada",
                table: "Workspaces",
                column: "StatusId",
                principalSchema: "Espada",
                principalTable: "WorkspaceStatusTypes",
                principalColumn: "WorkspaceStatusTypeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Workspaces_WorkspaceTypes_TypeId",
                schema: "Espada",
                table: "Workspaces",
                column: "TypeId",
                principalSchema: "Espada",
                principalTable: "WorkspaceTypes",
                principalColumn: "WorkspaceTypeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Artifacts_ArtifactStatusTypes_StatusId",
                schema: "Espada",
                table: "Artifacts");

            migrationBuilder.DropForeignKey(
                name: "FK_Artifacts_ArtifactTypes_TypeId",
                schema: "Espada",
                table: "Artifacts");

            migrationBuilder.DropForeignKey(
                name: "FK_ChunkBatches_ChunkBatchStatusTypes_StatusId",
                schema: "Espada",
                table: "ChunkBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_ChunkBatches_ChunkingStrategyTypes_StrategyId",
                schema: "Espada",
                table: "ChunkBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_Chunks_ChunkingStrategyTypes_StrategyId",
                schema: "Espada",
                table: "Chunks");

            migrationBuilder.DropForeignKey(
                name: "FK_ImportJobs_ImportStatusTypes_StatusId",
                schema: "Espada",
                table: "ImportJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_Sources_SourceStatusTypes_StatusId",
                schema: "Espada",
                table: "Sources");

            migrationBuilder.DropForeignKey(
                name: "FK_Sources_SourceTypes_TypeId",
                schema: "Espada",
                table: "Sources");

            migrationBuilder.DropForeignKey(
                name: "FK_Workspaces_WorkspaceStatusTypes_StatusId",
                schema: "Espada",
                table: "Workspaces");

            migrationBuilder.DropForeignKey(
                name: "FK_Workspaces_WorkspaceTypes_TypeId",
                schema: "Espada",
                table: "Workspaces");

            migrationBuilder.DropTable(
                name: "ArtifactStatusTypes",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "ArtifactTypes",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "ChunkBatchStatusTypes",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "ChunkingStrategyTypes",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "ImportStatusTypes",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "SourceStatusTypes",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "SourceTypes",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "WorkspaceStatusTypes",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "WorkspaceTypes",
                schema: "Espada");

            migrationBuilder.DropIndex(
                name: "IX_Workspaces_TypeId",
                schema: "Espada",
                table: "Workspaces");

            migrationBuilder.DropIndex(
                name: "IX_Sources_TypeId",
                schema: "Espada",
                table: "Sources");

            migrationBuilder.DropIndex(
                name: "IX_Chunks_StrategyId",
                schema: "Espada",
                table: "Chunks");

            migrationBuilder.DropIndex(
                name: "IX_ChunkBatches_StrategyId",
                schema: "Espada",
                table: "ChunkBatches");

            migrationBuilder.DropIndex(
                name: "IX_Artifacts_TypeId",
                schema: "Espada",
                table: "Artifacts");
        }
    }
}