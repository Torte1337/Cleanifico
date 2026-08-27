using Cleanifico.Domain.EmployeeContracts;

namespace Cleanifico.Application.EmployeeContracts;

public sealed record EmployeeContractRecord(
    EmployeeContract Contract,
    string EmployeeNumber,
    string EmployeeName);
