using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Espada.Db.Migrations
{
    /// <summary>
    /// Adds device-local derived repository manifest entries.
    /// Recovery: dropping the table is safe because a Git-aware full rescan recreates every entry.
    /// </summary>
    public partial class RepositoryManifest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RepositoryManifestEntries",
                schema: "Espada",
                columns: table => new
                {
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelativePath = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: false),
                    ContentHash = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    FileName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    MediaType = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    SizeInBytes = table.Column<long>(type: "bigint", nullable: false),
                    ScannedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepositoryManifestEntries", x => new { x.SourceId, x.RelativePath });
                    table.ForeignKey(
                        name: "FK_RepositoryManifestEntries_Sources_SourceId",
                        column: x => x.SourceId,
                        principalSchema: "Espada",
                        principalTable: "Sources",
                        principalColumn: "SourceId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RepositoryManifestEntries",
                schema: "Espada");
        }
    }
}