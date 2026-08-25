using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cleanifico.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigurableTimeTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataInitializationMarkers",
                columns: table => new
                {
                    Key = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataInitializationMarkers", x => x.Key);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TimeTypes",
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
                    CountsAsWorkTime = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    IsPaid = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    RequiresObject = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    IsAbsence = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    Color = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: true, collation: "ascii_general_ci")
                        .Annotation("MySql:CharSet", "ascii"),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeTypes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TimeTypes_Status_SortOrder_Name",
                table: "TimeTypes",
                columns: new[] { "IsActive", "SortOrder", "Name" });

            migrationBuilder.CreateIndex(
                name: "UX_TimeTypes_Code",
                table: "TimeTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_TimeTypes_Name",
                table: "TimeTypes",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataInitializationMarkers");

            migrationBuilder.DropTable(
                name: "TimeTypes");
        }
    }
}
