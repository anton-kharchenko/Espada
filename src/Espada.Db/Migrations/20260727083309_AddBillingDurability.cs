using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Espada.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddBillingDurability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_BillingCustomers_ProviderCustomerId",
                schema: "Espada",
                table: "BillingCustomers",
                column: "ProviderCustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentEvents_Status_AvailableAtUtc_LeaseExpiresAtUtc",
                schema: "Espada",
                table: "PaymentEvents",
                columns: new[] { "Status", "AvailableAtUtc", "LeaseExpiresAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillingCustomers",
                schema: "Espada");

            migrationBuilder.DropTable(
                name: "PaymentEvents",
                schema: "Espada");
        }
    }
}