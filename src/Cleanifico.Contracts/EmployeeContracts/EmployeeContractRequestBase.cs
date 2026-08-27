using System.ComponentModel.DataAnnotations;

namespace Cleanifico.Contracts.EmployeeContracts;

public abstract class EmployeeContractRequestBase
{
    [Required(ErrorMessage = "Die Vertragsnummer ist erforderlich.")]
    [StringLength(50, ErrorMessage = "Die Vertragsnummer darf höchstens 50 Zeichen lang sein.")]
    public string ContractNumber { get; set; } = string.Empty;

    public Guid EmployeeId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsPermanent { get; set; }

    [StringLength(100, ErrorMessage = "Die Beschäftigungsart darf höchstens 100 Zeichen lang sein.")]
    public string? EmploymentType { get; set; }

    [Range(typeof(decimal), "0", "99999.99", ErrorMessage = "Die Wochenstunden dürfen nicht negativ sein.")]
    public decimal WeeklyHours { get; set; }

    [Range(typeof(decimal), "0", "99999.99", ErrorMessage = "Die monatlichen Sollstunden dürfen nicht negativ sein.")]
    public decimal MonthlyTargetHours { get; set; }

    [Range(typeof(decimal), "0", "999.99", ErrorMessage = "Die Urlaubstage dürfen nicht negativ sein.")]
    public decimal VacationDaysPerYear { get; set; }

    public DateOnly? ProbationEndDate { get; set; }

    [StringLength(2_000, ErrorMessage = "Die Notizen dürfen höchstens 2000 Zeichen lang sein.")]
    public string? Notes { get; set; }
}
