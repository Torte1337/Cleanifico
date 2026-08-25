using Cleanifico.Application.CleaningTypes;
using Cleanifico.Contracts.CleaningTypes;
using Cleanifico.Contracts.Security;
using Cleanifico.Domain.CleaningTypes;

namespace Cleanifico.Api.Endpoints;

public static class CleaningTypeEndpoints
{
    public static IEndpointRouteBuilder MapCleaningTypeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/cleaning-types")
            .WithTags("Cleaning Types");

        group.MapGet("", GetAllAsync)
            .RequireAuthorization(SecurityPolicies.ViewCleaningTypes);
        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetCleaningType")
            .RequireAuthorization(SecurityPolicies.ViewCleaningTypes);
        group.MapPost("", CreateAsync)
            .RequireAuthorization(SecurityPolicies.ManageCleaningTypes);
        group.MapPut("/{id:guid}", UpdateAsync)
            .RequireAuthorization(SecurityPolicies.ManageCleaningTypes);
        group.MapPost("/{id:guid}/activate", ActivateAsync)
            .RequireAuthorization(SecurityPolicies.ManageCleaningTypes);
        group.MapPost("/{id:guid}/deactivate", DeactivateAsync)
            .RequireAuthorization(SecurityPolicies.ManageCleaningTypes);
        group.MapDelete("/{id:guid}", DeleteAsync)
            .RequireAuthorization(SecurityPolicies.ManageCleaningTypes);

        return endpoints;
    }

    private static async Task<IResult> GetAllAsync(
        string? search,
        bool? isActive,
        ICleaningTypeService service,
        CancellationToken cancellationToken)
    {
        var cleaningTypes = await service.GetAllAsync(search, isActive, cancellationToken);
        return Results.Ok(cleaningTypes.Select(ToResponse));
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        ICleaningTypeService service,
        CancellationToken cancellationToken)
    {
        var cleaningType = await service.GetByIdAsync(id, cancellationToken);
        return Results.Ok(ToResponse(cleaningType));
    }

    private static async Task<IResult> CreateAsync(
        CreateCleaningTypeRequest request,
        ICleaningTypeService service,
        CancellationToken cancellationToken)
    {
        var cleaningType = await service.CreateAsync(ToInput(request), cancellationToken);
        var response = ToResponse(cleaningType);

        return Results.Created($"/api/cleaning-types/{cleaningType.Id}", response);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateCleaningTypeRequest request,
        ICleaningTypeService service,
        CancellationToken cancellationToken)
    {
        var cleaningType = await service.UpdateAsync(id, ToInput(request), cancellationToken);
        return Results.Ok(ToResponse(cleaningType));
    }

    private static async Task<IResult> ActivateAsync(
        Guid id,
        ICleaningTypeService service,
        CancellationToken cancellationToken)
    {
        await service.ActivateAsync(id, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> DeactivateAsync(
        Guid id,
        ICleaningTypeService service,
        CancellationToken cancellationToken)
    {
        await service.DeactivateAsync(id, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        ICleaningTypeService service,
        CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return Results.NoContent();
    }

    private static CleaningTypeInput ToInput(CreateCleaningTypeRequest request) =>
        new(request.Name, request.Code, request.Description, request.SortOrder);

    private static CleaningTypeInput ToInput(UpdateCleaningTypeRequest request) =>
        new(request.Name, request.Code, request.Description, request.SortOrder);

    private static CleaningTypeResponse ToResponse(CleaningType cleaningType) =>
        new(
            cleaningType.Id,
            cleaningType.Name,
            cleaningType.Code,
            cleaningType.Description,
            cleaningType.IsActive,
            cleaningType.SortOrder,
            cleaningType.CreatedAtUtc,
            cleaningType.UpdatedAtUtc);
}
