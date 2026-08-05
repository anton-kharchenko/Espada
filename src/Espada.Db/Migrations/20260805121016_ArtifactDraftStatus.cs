using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Espada.Db.Migrations
{
    /// <summary>
    /// Adds the canonical Draft artifact status used by saved agent responses.
    /// Recovery: publish, archive, or export draft artifacts before rollback; the foreign key blocks unsafe removal.
    /// </summary>
    public partial class ArtifactDraftStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "Espada",
                table: "ArtifactStatusTypes",
                columns: new[] { "ArtifactStatusTypeId", "Name" },
                values: new object[] { 3, "Draft" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "Espada",
                table: "ArtifactStatusTypes",
                keyColumn: "ArtifactStatusTypeId",
                keyValue: 3);
        }
    }
}