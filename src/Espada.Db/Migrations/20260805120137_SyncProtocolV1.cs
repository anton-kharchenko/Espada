using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

#nullable disable

namespace Espada.Db.Migrations
{
    /// <summary>
    /// Adds server sequencing, push cursors, and cloud ownership for Sync Protocol v1.
    /// Recovery: stop sync clients and restore a pre-migration backup before rolling back.
    /// Pending local events remain in SyncEvents; export them before a destructive rollback.
    /// </summary>
    public partial class SyncProtocolV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ServerSequence",
                schema: "Espada",
                table: "SyncEvents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

            migrationBuilder.AddColumn<long>(
                name: "LastPushedSequence",
                schema: "Espada",
                table: "SyncCursors",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "SyncDeviceRegistrations",
                schema: "Espada",
                columns: table => new
                {
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Issuer = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RegisteredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncDeviceRegistrations", x => x.DeviceId);
                    table.ForeignKey(
                        name: "FK_SyncDeviceRegistrations_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalSchema: "Espada",
                        principalTable: "Devices",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UX_SyncEvents_ServerSequence",
                schema: "Espada",
                table: "SyncEvents",
                column: "ServerSequence",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyncDeviceRegistrations_Issuer_Subject",
                schema: "Espada",
                table: "SyncDeviceRegistrations",
                columns: new[] { "Issuer", "Subject" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SyncDeviceRegistrations",
                schema: "Espada");

            migrationBuilder.DropIndex(
                name: "UX_SyncEvents_ServerSequence",
                schema: "Espada",
                table: "SyncEvents");

            migrationBuilder.DropColumn(
                name: "ServerSequence",
                schema: "Espada",
                table: "SyncEvents");

            migrationBuilder.DropColumn(
                name: "LastPushedSequence",
                schema: "Espada",
                table: "SyncCursors");
        }
    }
}