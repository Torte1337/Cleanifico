using Cleanifico.Application.EmployeeContracts;
using Cleanifico.Contracts.EmployeeContracts;
using Cleanifico.Contracts.Security;

namespace Cleanifico.Api.Endpoints;

public static class EmployeeContractEndpoints
{
    public static IEndpointRouteBuilder MapEmployeeContractEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/employee-contracts")
            .WithTags("Employee Contracts");
        group.MapGet("", GetAllAsync)
            .RequireAuthorization(SecurityPolicies.ViewEmployeeContracts);
        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetEmployeeContract")
            .RequireAuthorization(SecurityPolicies.ViewEmployeeContracts);
        group.MapPost("", CreateAsync)
            .RequireAuthorization(SecurityPolicies.ManageEmployeeContracts);
        group.MapPut("/{id:guid}", UpdateAsync)
            .RequireAuthorization(SecurityPolicies.ManageEmployeeContracts);
        group.MapPost("/{id:guid}/activate", ActivateAsync)
            .RequireAuthorization(SecurityPolicies.ManageEmployeeContracts);
        group.MapPost("/{id:guid}/deactivate", DeactivateAsync)
            .RequireAuthorization(SecurityPolicies.ManageEmployeeContracts);
        group.MapDelete("/{id:guid}", DeleteAsync)
            .RequireAuthorization(SecurityPolicies.ManageEmployeeContracts);
        return endpoints;
    }

    private static async Task<IResult> GetAllAsync(
        string? search,
        bool? isActive,
        Guid? employeeId,
        IEmployeeContractService service,
        CancellationToken cancellationToken) =>
        Results.Ok((await service.GetAllAsync(
            search,
            isActive,
            employeeId,
            cancellationToken)).Select(ToResponse));

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        IEmployeeContractService service,
        CancellationToken cancellationToken) =>
        Results.Ok(ToResponse(await service.GetByIdAsync(id, cancellationToken)));

    private static async Task<IResult> CreateAsync(
        CreateEmployeeContractRequest request,
        IEmployeeContractService service,
        CancellationToken cancellationToken)
    {
        EmployeeContractRecord result = await service.CreateAsync(ToInput(request), cancellationToken);
        return Results.Created($"/api/employee-contracts/{result.Contract.Id}", ToResponse(result));
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateEmployeeContractRequest request,
        IEmployeeContractService service,
        CancellationToken cancellationToken) =>
        Results.Ok(ToResponse(await service.UpdateAsync(id, ToInput(request), cancellationToken)));

    private static async Task<IResult> ActivateAsync(
        Guid id,
        IEmployeeContractService service,
        CancellationToken cancellationToken)
    {
        await service.ActivateAsync(id, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> DeactivateAsync(
        Guid id,
        IEmployeeContractService service,
        CancellationToken cancellationToken)
    {
        await service.DeactivateAsync(id, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        IEmployeeContractService service,
        CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return Results.NoContent();
    }

    private static EmployeeContractInput ToInput(EmployeeContractRequestBase request) => new(
        request.ContractNumber,
        request.EmployeeId,
        request.StartDate,
        request.EndDate,
        request.IsPermanent,
        request.EmploymentType,
        request.WeeklyHours,
        request.MonthlyTargetHours,
        request.VacationDaysPerYear,
        request.ProbationEndDate,
        request.Notes);

    private static EmployeeContractResponse ToResponse(EmployeeContractRecord record)
    {
        var contract = record.Contract;
        return new EmployeeContractResponse(
            contract.Id,
            contract.ContractNumber,
            contract.EmployeeId,
            record.EmployeeNumber,
            record.EmployeeName,
            contract.StartDate,
            contract.EndDate,
            contract.IsPermanent,
            contract.EmploymentType,
            contract.WeeklyHours,
            contract.MonthlyTargetHours,
            contract.VacationDaysPerYear,
            contract.ProbationEndDate,
            contract.Notes,
            contract.IsActive,
            contract.CreatedAtUtc,
            contract.UpdatedAtUtc);
    }
}
