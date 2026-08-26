using Cleanifico.Application.Customers;
using Cleanifico.Contracts.Customers;
using Cleanifico.Contracts.Security;
using Cleanifico.Domain.Customers;

namespace Cleanifico.Api.Endpoints;

public static class CustomerEndpoints
{
    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/customers")
            .WithTags("Customers");

        group.MapGet("", GetAllAsync)
            .RequireAuthorization(SecurityPolicies.ViewCustomers);
        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetCustomer")
            .RequireAuthorization(SecurityPolicies.ViewCustomers);
        group.MapPost("", CreateAsync)
            .RequireAuthorization(SecurityPolicies.ManageCustomers);
        group.MapPut("/{id:guid}", UpdateAsync)
            .RequireAuthorization(SecurityPolicies.ManageCustomers);
        group.MapPost("/{id:guid}/activate", ActivateAsync)
            .RequireAuthorization(SecurityPolicies.ManageCustomers);
        group.MapPost("/{id:guid}/deactivate", DeactivateAsync)
            .RequireAuthorization(SecurityPolicies.ManageCustomers);
        group.MapDelete("/{id:guid}", DeleteAsync)
            .RequireAuthorization(SecurityPolicies.ManageCustomers);

        return endpoints;
    }

    private static async Task<IResult> GetAllAsync(
        string? search,
        bool? isActive,
        ICustomerService service,
        CancellationToken cancellationToken)
    {
        var customers = await service.GetAllAsync(search, isActive, cancellationToken);
        return Results.Ok(customers.Select(ToResponse));
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        ICustomerService service,
        CancellationToken cancellationToken)
    {
        var customer = await service.GetByIdAsync(id, cancellationToken);
        return Results.Ok(ToResponse(customer));
    }

    private static async Task<IResult> CreateAsync(
        CreateCustomerRequest request,
        ICustomerService service,
        CancellationToken cancellationToken)
    {
        var customer = await service.CreateAsync(ToInput(request), cancellationToken);
        return Results.Created($"/api/customers/{customer.Id}", ToResponse(customer));
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateCustomerRequest request,
        ICustomerService service,
        CancellationToken cancellationToken)
    {
        var customer = await service.UpdateAsync(id, ToInput(request), cancellationToken);
        return Results.Ok(ToResponse(customer));
    }

    private static async Task<IResult> ActivateAsync(
        Guid id,
        ICustomerService service,
        CancellationToken cancellationToken)
    {
        await service.ActivateAsync(id, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> DeactivateAsync(
        Guid id,
        ICustomerService service,
        CancellationToken cancellationToken)
    {
        await service.DeactivateAsync(id, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        ICustomerService service,
        CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return Results.NoContent();
    }

    private static CustomerInput ToInput(CreateCustomerRequest request) =>
        new(
            request.CustomerNumber,
            request.CompanyName,
            request.ContactFirstName,
            request.ContactLastName,
            request.Email,
            request.Phone,
            request.Street,
            request.PostalCode,
            request.City,
            request.Country,
            request.Notes);

    private static CustomerInput ToInput(UpdateCustomerRequest request) =>
        new(
            request.CustomerNumber,
            request.CompanyName,
            request.ContactFirstName,
            request.ContactLastName,
            request.Email,
            request.Phone,
            request.Street,
            request.PostalCode,
            request.City,
            request.Country,
            request.Notes);

    private static CustomerResponse ToResponse(Customer customer) =>
        new(
            customer.Id,
            customer.CustomerNumber,
            customer.CompanyName,
            customer.ContactFirstName,
            customer.ContactLastName,
            customer.Email,
            customer.Phone,
            customer.Street,
            customer.PostalCode,
            customer.City,
            customer.Country,
            customer.Notes,
            customer.IsActive,
            customer.CreatedAtUtc,
            customer.UpdatedAtUtc);
}
