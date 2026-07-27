using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Espada.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspaceMemberships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkspaceMemberships",
                schema: "Espada",
                columns: table => new
                {
                    WorkspaceMembershipId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Issuer = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    JoinedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkspaceMemberships", x => x.WorkspaceMembershipId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceMemberships_WorkspaceId_Issuer_Subject",
                schema: "Espada",
                table: "WorkspaceMemberships",
                columns: new[] { "WorkspaceId", "Issuer", "Subject" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkspaceMemberships",
                schema: "Espada");
        }
    }
}