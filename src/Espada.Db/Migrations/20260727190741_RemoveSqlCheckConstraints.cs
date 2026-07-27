using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Espada.Db.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSqlCheckConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Sources_Priority_Range",
                schema: "Espada",
                table: "Sources");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Artifacts_Priority_Range",
                schema: "Espada",
                table: "Artifacts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException(
                "This migration removes SQL-defined check constraints. "
                + "Restore the pre-migration database backup to roll back.");
        }
    }
}