using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Espada.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspaceMembershipForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_WorkspaceMemberships_Workspaces_WorkspaceId",
                schema: "Espada",
                table: "WorkspaceMemberships",
                column: "WorkspaceId",
                principalSchema: "Espada",
                principalTable: "Workspaces",
                principalColumn: "WorkspaceId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkspaceMemberships_Workspaces_WorkspaceId",
                schema: "Espada",
                table: "WorkspaceMemberships");
        }
    }
}