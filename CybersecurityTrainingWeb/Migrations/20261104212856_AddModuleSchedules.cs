using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CybersecurityTrainingWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddModuleSchedules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModuleSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ModuleId = table.Column<int>(type: "INTEGER", nullable: false),
                    FirstDueAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RepeatEveryDays = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleSchedules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModuleSchedules_ModuleId",
                table: "ModuleSchedules",
                column: "ModuleId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModuleSchedules");
        }
    }
}
