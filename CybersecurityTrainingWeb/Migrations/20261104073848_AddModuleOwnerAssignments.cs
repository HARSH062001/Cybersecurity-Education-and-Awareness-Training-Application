using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CybersecurityTrainingWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddModuleOwnerAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModuleOwnerAssignments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ModuleId = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnerUserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleOwnerAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModuleOwnerAssignments_AspNetUsers_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModuleOwnerAssignments_ModuleId_OwnerUserId",
                table: "ModuleOwnerAssignments",
                columns: new[] { "ModuleId", "OwnerUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModuleOwnerAssignments_OwnerUserId",
                table: "ModuleOwnerAssignments",
                column: "OwnerUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModuleOwnerAssignments");
        }
    }
}
