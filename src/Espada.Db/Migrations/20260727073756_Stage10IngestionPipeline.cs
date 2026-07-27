using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Espada.Db.Migrations
{
    /// <inheritdoc />
    public partial class Stage10IngestionPipeline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefinitionJson",
                schema: "Espada",
                table: "Sources",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                schema: "Espada",
                table: "ImportJobs",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValueSql: "gen_random_uuid()::text");

            migrationBuilder.AddColumn<string>(
                name: "OptionsJson",
                schema: "Espada",
                table: "ImportJobs",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "RequestFingerprint",
                schema: "Espada",
                table: "ImportJobs",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValueSql: "gen_random_uuid()::text");

            migrationBuilder.AddColumn<int>(
                name: "Stage",
                schema: "Espada",
                table: "ImportJobs",
                type: "integer",
                nullable: false,
                defaultValue: 1);

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

            migrationBuilder.CreateIndex(
                name: "UX_ImportJobs_WorkspaceId_IdempotencyKey",
                schema: "Espada",
                table: "ImportJobs",
                columns: new[] { "WorkspaceId", "IdempotencyKey" },
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
                name: "IX_OutboxMessages_ProcessedAtUtc_AvailableAtUtc",
                schema: "Espada",
                table: "OutboxMessages",
                columns: new[] { "ProcessedAtUtc", "AvailableAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IngestionJobs",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "Espada");

            migrationBuilder.DropIndex(
                name: "UX_ImportJobs_WorkspaceId_IdempotencyKey",
                schema: "Espada",
                table: "ImportJobs");

            migrationBuilder.DropColumn(
                name: "DefinitionJson",
                schema: "Espada",
                table: "Sources");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                schema: "Espada",
                table: "ImportJobs");

            migrationBuilder.DropColumn(
                name: "OptionsJson",
                schema: "Espada",
                table: "ImportJobs");

            migrationBuilder.DropColumn(
                name: "RequestFingerprint",
                schema: "Espada",
                table: "ImportJobs");

            migrationBuilder.DropColumn(
                name: "Stage",
                schema: "Espada",
                table: "ImportJobs");
        }
    }
}