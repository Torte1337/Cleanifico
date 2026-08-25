using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cleanifico.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCleanificoPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CleaningTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", fixedLength: true, nullable: false, collation: "ascii_general_ci")
                        .Annotation("MySql:CharSet", "ascii"),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CleaningTypes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CleaningTypes_Status_SortOrder_Name",
                table: "CleaningTypes",
                columns: new[] { "IsActive", "SortOrder", "Name" });

            migrationBuilder.CreateIndex(
                name: "UX_CleaningTypes_Code",
                table: "CleaningTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_CleaningTypes_Name",
                table: "CleaningTypes",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CleaningTypes");
        }
    }
}
