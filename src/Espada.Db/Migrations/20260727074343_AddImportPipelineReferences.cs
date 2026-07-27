using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Espada.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddImportPipelineReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ChunkBatchId",
                schema: "Espada",
                table: "ImportJobs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParsedBlobHash",
                schema: "Espada",
                table: "ImportJobs",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RawBlobHash",
                schema: "Espada",
                table: "ImportJobs",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChunkBatchId",
                schema: "Espada",
                table: "ImportJobs");

            migrationBuilder.DropColumn(
                name: "ParsedBlobHash",
                schema: "Espada",
                table: "ImportJobs");

            migrationBuilder.DropColumn(
                name: "RawBlobHash",
                schema: "Espada",
                table: "ImportJobs");
        }
    }
}