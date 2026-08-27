namespace Cleanifico.Contracts.EmployeeContracts;

public sealed record EmployeeContractResponse(
    Guid Id,
    string ContractNumber,
    Guid EmployeeId,
    string EmployeeNumber,
    string EmployeeName,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsPermanent,
    string? EmploymentType,
    decimal WeeklyHours,
    decimal MonthlyTargetHours,
    decimal VacationDaysPerYear,
    DateOnly? ProbationEndDate,
    string? Notes,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
