using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Espada.Db.Migrations
{
    /// <inheritdoc />
    public partial class UsePostgreSqlXminConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}