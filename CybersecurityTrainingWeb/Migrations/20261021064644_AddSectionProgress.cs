using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CybersecurityTrainingWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddSectionProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SectionProgressEntries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ModuleId = table.Column<int>(type: "INTEGER", nullable: false),
                    SectionKey = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectionProgressEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SectionProgressEntries_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SectionProgressEntries_UserId_ModuleId_SectionKey",
                table: "SectionProgressEntries",
                columns: new[] { "UserId", "ModuleId", "SectionKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SectionProgressEntries");
        }
    }
}
