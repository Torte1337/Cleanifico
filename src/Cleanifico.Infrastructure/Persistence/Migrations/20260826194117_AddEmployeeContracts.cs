using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cleanifico.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeContracts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeContracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", fixedLength: true, nullable: false, collation: "ascii_general_ci")
                        .Annotation("MySql:CharSet", "ascii"),
                    ContractNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmployeeId = table.Column<Guid>(type: "char(36)", fixedLength: true, nullable: false, collation: "ascii_general_ci")
                        .Annotation("MySql:CharSet", "ascii"),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsPermanent = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EmploymentType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WeeklyHours = table.Column<decimal>(type: "decimal(7,2)", precision: 7, scale: 2, nullable: false),
                    MonthlyTargetHours = table.Column<decimal>(type: "decimal(7,2)", precision: 7, scale: 2, nullable: false),
                    VacationDaysPerYear = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ProbationEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeContracts_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeContracts_Employee_Status_Period",
                table: "EmployeeContracts",
                columns: new[] { "EmployeeId", "IsActive", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeContracts_Status_StartDate",
                table: "EmployeeContracts",
                columns: new[] { "IsActive", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "UX_EmployeeContracts_ContractNumber",
                table: "EmployeeContracts",
                column: "ContractNumber",
                unique: true);

            migrationBuilder.Sql(
                """
                INSERT INTO EmployeeContracts
                    (Id, ContractNumber, EmployeeId, StartDate, EndDate, IsPermanent,
                     EmploymentType, WeeklyHours, MonthlyTargetHours, VacationDaysPerYear,
                     ProbationEndDate, Notes, IsActive, CreatedAtUtc, UpdatedAtUtc)
                SELECT
                    e.Id,
                    CONCAT('MIG-', REPLACE(CAST(e.Id AS CHAR), '-', '')),
                    e.Id,
                    COALESCE(e.EmploymentStartDate, e.EmploymentEndDate, DATE(e.CreatedAtUtc)),
                    e.EmploymentEndDate,
                    CASE WHEN e.EmploymentEndDate IS NULL THEN 1 ELSE 0 END,
                    e.EmploymentType,
                    e.WeeklyHours,
                    e.MonthlyTargetHours,
                    0,
                    NULL,
                    CASE
                        WHEN e.EmploymentStartDate IS NULL
                        THEN 'Automatisch aus Personalstammdaten übernommen; der bisherige Beschäftigungsbeginn war nicht gepflegt und wurde aus dem vorhandenen Ende beziehungsweise dem Erstellungsdatum abgeleitet.'
                        ELSE NULL
                    END,
                    e.IsActive,
                    e.CreatedAtUtc,
                    e.UpdatedAtUtc
                FROM Employees e
                WHERE e.EmploymentStartDate IS NOT NULL
                   OR e.EmploymentEndDate IS NOT NULL
                   OR NULLIF(TRIM(e.EmploymentType), '') IS NOT NULL
                   OR e.WeeklyHours <> 0
                   OR e.MonthlyTargetHours <> 0;
                """);

            migrationBuilder.DropColumn(
                name: "EmploymentEndDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmploymentStartDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmploymentType",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "MonthlyTargetHours",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "WeeklyHours",
                table: "Employees");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "EmploymentEndDate",
                table: "Employees",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "EmploymentStartDate",
                table: "Employees",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmploymentType",
                table: "Employees",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyTargetHours",
                table: "Employees",
                type: "decimal(7,2)",
                precision: 7,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "WeeklyHours",
                table: "Employees",
                type: "decimal(7,2)",
                precision: 7,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(
                """
                UPDATE Employees e
                SET EmploymentStartDate = (
                        SELECT c.StartDate FROM EmployeeContracts c
                        WHERE c.EmployeeId = e.Id
                        ORDER BY c.IsActive DESC, c.StartDate DESC LIMIT 1),
                    EmploymentEndDate = (
                        SELECT c.EndDate FROM EmployeeContracts c
                        WHERE c.EmployeeId = e.Id
                        ORDER BY c.IsActive DESC, c.StartDate DESC LIMIT 1),
                    EmploymentType = (
                        SELECT c.EmploymentType FROM EmployeeContracts c
                        WHERE c.EmployeeId = e.Id
                        ORDER BY c.IsActive DESC, c.StartDate DESC LIMIT 1),
                    WeeklyHours = COALESCE((
                        SELECT c.WeeklyHours FROM EmployeeContracts c
                        WHERE c.EmployeeId = e.Id
                        ORDER BY c.IsActive DESC, c.StartDate DESC LIMIT 1), 0),
                    MonthlyTargetHours = COALESCE((
                        SELECT c.MonthlyTargetHours FROM EmployeeContracts c
                        WHERE c.EmployeeId = e.Id
                        ORDER BY c.IsActive DESC, c.StartDate DESC LIMIT 1), 0)
                WHERE EXISTS (
                    SELECT 1 FROM EmployeeContracts c WHERE c.EmployeeId = e.Id);
                """);

            migrationBuilder.DropTable(
                name: "EmployeeContracts");
        }
    }
}
