using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Espada.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddUsageReconciliation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsageReconciliationOutbox",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "UsageLedgerEntries",
                schema: "Espada");
        }
    }
}