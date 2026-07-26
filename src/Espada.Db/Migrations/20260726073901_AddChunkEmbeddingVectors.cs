using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Espada.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddChunkEmbeddingVectors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChunkEmbeddingVectors",
                schema: "Espada",
                columns: table => new
                {
                    ChunkEmbeddingId = table.Column<Guid>(type: "uuid", nullable: false),
                    Vector = table.Column<float[]>(type: "real[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChunkEmbeddingVectors", x => x.ChunkEmbeddingId);
                    table.ForeignKey(
                        name: "FK_ChunkEmbeddingVectors_ChunkEmbeddings_ChunkEmbeddingId",
                        column: x => x.ChunkEmbeddingId,
                        principalSchema: "Espada",
                        principalTable: "ChunkEmbeddings",
                        principalColumn: "ChunkEmbeddingId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChunkEmbeddingVectors",
                schema: "Espada");
        }
    }
}
