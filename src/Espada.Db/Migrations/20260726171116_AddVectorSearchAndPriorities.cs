using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Espada.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddVectorSearchAndPriorities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                schema: "Espada",
                table: "Sources",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                ALTER TABLE "Espada"."ChunkEmbeddingVectors"
                    ALTER COLUMN "Vector" TYPE vector
                    USING "Vector"::vector;
                """);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                schema: "Espada",
                table: "Artifacts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Sources_Priority_Range",
                schema: "Espada",
                table: "Sources",
                sql: "\"Priority\" BETWEEN -100 AND 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Artifacts_Priority_Range",
                schema: "Espada",
                table: "Artifacts",
                sql: "\"Priority\" BETWEEN -100 AND 100");

            migrationBuilder.Sql(
                """
                CREATE INDEX "IX_Chunks_Content_Fts"
                    ON "Espada"."Chunks"
                    USING GIN (to_tsvector('simple', "Content"));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP INDEX IF EXISTS "Espada"."IX_Chunks_Content_Fts";
                """);

            migrationBuilder.DropCheckConstraint(
                name: "CK_Sources_Priority_Range",
                schema: "Espada",
                table: "Sources");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Artifacts_Priority_Range",
                schema: "Espada",
                table: "Artifacts");

            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "Espada",
                table: "Sources");

            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "Espada",
                table: "Artifacts");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.Sql(
                """
                ALTER TABLE "Espada"."ChunkEmbeddingVectors"
                    ALTER COLUMN "Vector" TYPE real[]
                    USING "Vector"::real[];
                """);
        }
    }
}