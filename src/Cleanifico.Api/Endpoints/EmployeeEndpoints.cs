using Cleanifico.Application.Employees;
using Cleanifico.Contracts.Employees;
using Cleanifico.Contracts.Security;
using Cleanifico.Domain.Employees;

namespace Cleanifico.Api.Endpoints;

public static class EmployeeEndpoints
{
    public static IEndpointRouteBuilder MapEmployeeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/employees").WithTags("Employees");
        group.MapGet("", GetAllAsync).RequireAuthorization(SecurityPolicies.ViewEmployees);
        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetEmployee")
            .RequireAuthorization(SecurityPolicies.ViewEmployees);
        group.MapPost("", CreateAsync).RequireAuthorization(SecurityPolicies.ManageEmployees);
        group.MapPut("/{id:guid}", UpdateAsync).RequireAuthorization(SecurityPolicies.ManageEmployees);
        group.MapPost("/{id:guid}/activate", ActivateAsync)
            .RequireAuthorization(SecurityPolicies.ManageEmployees);
        group.MapPost("/{id:guid}/deactivate", DeactivateAsync)
            .RequireAuthorization(SecurityPolicies.ManageEmployees);
        group.MapDelete("/{id:guid}", DeleteAsync)
            .RequireAuthorization(SecurityPolicies.ManageEmployees);
        return endpoints;
    }

    private static async Task<IResult> GetAllAsync(
        string? search,
        bool? isActive,
        IEmployeeService service,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Employee> employees = await service.GetAllAsync(search, isActive, cancellationToken);
        return Results.Ok(employees.Select(ToResponse));
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        IEmployeeService service,
        CancellationToken cancellationToken) =>
        Results.Ok(ToResponse(await service.GetByIdAsync(id, cancellationToken)));

    private static async Task<IResult> CreateAsync(
        CreateEmployeeRequest request,
        IEmployeeService service,
        CancellationToken cancellationToken)
    {
        Employee employee = await service.CreateAsync(ToInput(request), cancellationToken);
        return Results.Created($"/api/employees/{employee.Id}", ToResponse(employee));
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateEmployeeRequest request,
        IEmployeeService service,
        CancellationToken cancellationToken) =>
        Results.Ok(ToResponse(await service.UpdateAsync(id, ToInput(request), cancellationToken)));

    private static async Task<IResult> ActivateAsync(
        Guid id,
        IEmployeeService service,
        CancellationToken cancellationToken)
    {
        await service.ActivateAsync(id, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> DeactivateAsync(
        Guid id,
        IEmployeeService service,
        CancellationToken cancellationToken)
    {
        await service.DeactivateAsync(id, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        IEmployeeService service,
        CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return Results.NoContent();
    }

    private static EmployeeInput ToInput(EmployeeRequestBase request) => new(
        request.EmployeeNumber,
        request.FirstName,
        request.LastName,
        request.Street,
        request.PostalCode,
        request.City,
        request.Country,
        request.Email,
        request.Phone,
        request.MobilePhone,
        request.DateOfBirth,
        request.EmploymentStartDate,
        request.EmploymentEndDate,
        request.EmploymentType,
        request.WeeklyHours,
        request.MonthlyTargetHours,
        request.Notes);

    private static EmployeeResponse ToResponse(Employee employee) => new(
        employee.Id,
        employee.EmployeeNumber,
        employee.FirstName,
        employee.LastName,
        employee.Street,
        employee.PostalCode,
        employee.City,
        employee.Country,
        employee.Email,
        employee.Phone,
        employee.MobilePhone,
        employee.DateOfBirth,
        employee.EmploymentStartDate,
        employee.EmploymentEndDate,
        employee.EmploymentType,
        employee.WeeklyHours,
        employee.MonthlyTargetHours,
        employee.Notes,
        employee.IsActive,
        employee.CreatedAtUtc,
        employee.UpdatedAtUtc);
}
