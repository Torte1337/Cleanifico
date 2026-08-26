using Cleanifico.Application.CleaningObjects;
using Cleanifico.Contracts.CleaningObjects;
using Cleanifico.Contracts.Security;

namespace Cleanifico.Api.Endpoints;

public static class CleaningObjectEndpoints
{
    public static IEndpointRouteBuilder MapCleaningObjectEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/objects").WithTags("Objects");
        group.MapGet("", GetAllAsync).RequireAuthorization(SecurityPolicies.ViewObjects);
        group.MapGet("/{id:guid}", GetByIdAsync).WithName("GetCleaningObject")
            .RequireAuthorization(SecurityPolicies.ViewObjects);
        group.MapPost("", CreateAsync).RequireAuthorization(SecurityPolicies.ManageObjects);
        group.MapPut("/{id:guid}", UpdateAsync).RequireAuthorization(SecurityPolicies.ManageObjects);
        group.MapPost("/{id:guid}/activate", ActivateAsync).RequireAuthorization(SecurityPolicies.ManageObjects);
        group.MapPost("/{id:guid}/deactivate", DeactivateAsync).RequireAuthorization(SecurityPolicies.ManageObjects);
        group.MapDelete("/{id:guid}", DeleteAsync).RequireAuthorization(SecurityPolicies.ManageObjects);
        return endpoints;
    }

    private static async Task<IResult> GetAllAsync(
        string? search,
        bool? isActive,
        Guid? customerId,
        ICleaningObjectService service,
        CancellationToken cancellationToken) =>
        Results.Ok((await service.GetAllAsync(search, isActive, customerId, cancellationToken)).Select(ToResponse));

    private static async Task<IResult> GetByIdAsync(Guid id, ICleaningObjectService service, CancellationToken cancellationToken) =>
        Results.Ok(ToResponse(await service.GetByIdAsync(id, cancellationToken)));

    private static async Task<IResult> CreateAsync(
        CreateCleaningObjectRequest request,
        ICleaningObjectService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(ToInput(request), cancellationToken);
        return Results.Created($"/api/objects/{result.CleaningObject.Id}", ToResponse(result));
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateCleaningObjectRequest request,
        ICleaningObjectService service,
        CancellationToken cancellationToken) =>
        Results.Ok(ToResponse(await service.UpdateAsync(id, ToInput(request), cancellationToken)));

    private static async Task<IResult> ActivateAsync(Guid id, ICleaningObjectService service, CancellationToken cancellationToken)
    {
        await service.ActivateAsync(id, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> DeactivateAsync(Guid id, ICleaningObjectService service, CancellationToken cancellationToken)
    {
        await service.DeactivateAsync(id, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteAsync(Guid id, ICleaningObjectService service, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return Results.NoContent();
    }

    private static CleaningObjectInput ToInput(CreateCleaningObjectRequest request) => new(
        request.ObjectNumber, request.CustomerId, request.Name, request.Street, request.PostalCode,
        request.City, request.Country, request.ContactFirstName, request.ContactLastName,
        request.ContactEmail, request.ContactPhone, request.AccessNotes, request.CleaningNotes);

    private static CleaningObjectInput ToInput(UpdateCleaningObjectRequest request) => new(
        request.ObjectNumber, request.CustomerId, request.Name, request.Street, request.PostalCode,
        request.City, request.Country, request.ContactFirstName, request.ContactLastName,
        request.ContactEmail, request.ContactPhone, request.AccessNotes, request.CleaningNotes);

    private static CleaningObjectResponse ToResponse(CleaningObjectRecord record)
    {
        var item = record.CleaningObject;
        return new CleaningObjectResponse(
            item.Id, item.ObjectNumber, item.CustomerId, record.CustomerNumber, record.CustomerCompanyName,
            item.Name, item.Street, item.PostalCode, item.City, item.Country, item.ContactFirstName,
            item.ContactLastName, item.ContactEmail, item.ContactPhone, item.AccessNotes, item.CleaningNotes,
            item.IsActive, item.CreatedAtUtc, item.UpdatedAtUtc);
    }
}
