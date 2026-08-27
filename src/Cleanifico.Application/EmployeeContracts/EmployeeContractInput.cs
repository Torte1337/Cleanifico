namespace Cleanifico.Application.EmployeeContracts;

public sealed record EmployeeContractInput(
    string? ContractNumber,
    Guid EmployeeId,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsPermanent,
    string? EmploymentType,
    decimal WeeklyHours,
    decimal MonthlyTargetHours,
    decimal VacationDaysPerYear,
    DateOnly? ProbationEndDate,
    string? Notes);
