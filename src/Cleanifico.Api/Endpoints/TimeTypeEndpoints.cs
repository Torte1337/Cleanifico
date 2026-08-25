using Cleanifico.Application.TimeTypes;
using Cleanifico.Contracts.Security;
using Cleanifico.Contracts.TimeTypes;
using Cleanifico.Domain.TimeTypes;

namespace Cleanifico.Api.Endpoints;

public static class TimeTypeEndpoints
{
    public static IEndpointRouteBuilder MapTimeTypeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/time-types")
            .WithTags("Time Types");

        group.MapGet("", GetAllAsync)
            .RequireAuthorization(SecurityPolicies.ViewTimeTypes);
        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetTimeType")
            .RequireAuthorization(SecurityPolicies.ViewTimeTypes);
        group.MapPost("", CreateAsync)
            .RequireAuthorization(SecurityPolicies.ManageTimeTypes);
        group.MapPut("/{id:guid}", UpdateAsync)
            .RequireAuthorization(SecurityPolicies.ManageTimeTypes);
        group.MapPost("/{id:guid}/activate", ActivateAsync)
            .RequireAuthorization(SecurityPolicies.ManageTimeTypes);
        group.MapPost("/{id:guid}/deactivate", DeactivateAsync)
            .RequireAuthorization(SecurityPolicies.ManageTimeTypes);
        group.MapDelete("/{id:guid}", DeleteAsync)
            .RequireAuthorization(SecurityPolicies.ManageTimeTypes);

        return endpoints;
    }

    private static async Task<IResult> GetAllAsync(
        string? search,
        bool? isActive,
        ITimeTypeService service,
        CancellationToken cancellationToken)
    {
        var timeTypes = await service.GetAllAsync(search, isActive, cancellationToken);
        return Results.Ok(timeTypes.Select(ToResponse));
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        ITimeTypeService service,
        CancellationToken cancellationToken)
    {
        var timeType = await service.GetByIdAsync(id, cancellationToken);
        return Results.Ok(ToResponse(timeType));
    }

    private static async Task<IResult> CreateAsync(
        CreateTimeTypeRequest request,
        ITimeTypeService service,
        CancellationToken cancellationToken)
    {
        var timeType = await service.CreateAsync(ToInput(request), cancellationToken);
        return Results.Created($"/api/time-types/{timeType.Id}", ToResponse(timeType));
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateTimeTypeRequest request,
        ITimeTypeService service,
        CancellationToken cancellationToken)
    {
        var timeType = await service.UpdateAsync(id, ToInput(request), cancellationToken);
        return Results.Ok(ToResponse(timeType));
    }

    private static async Task<IResult> ActivateAsync(
        Guid id,
        ITimeTypeService service,
        CancellationToken cancellationToken)
    {
        await service.ActivateAsync(id, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> DeactivateAsync(
        Guid id,
        ITimeTypeService service,
        CancellationToken cancellationToken)
    {
        await service.DeactivateAsync(id, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        ITimeTypeService service,
        CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return Results.NoContent();
    }

    private static TimeTypeInput ToInput(CreateTimeTypeRequest request) =>
        new(
            request.Name,
            request.Code,
            request.Description,
            request.CountsAsWorkTime,
            request.IsPaid,
            request.RequiresObject,
            request.IsAbsence,
            request.Color,
            request.SortOrder);

    private static TimeTypeInput ToInput(UpdateTimeTypeRequest request) =>
        new(
            request.Name,
            request.Code,
            request.Description,
            request.CountsAsWorkTime,
            request.IsPaid,
            request.RequiresObject,
            request.IsAbsence,
            request.Color,
            request.SortOrder);

    private static TimeTypeResponse ToResponse(TimeType timeType) =>
        new(
            timeType.Id,
            timeType.Name,
            timeType.Code,
            timeType.Description,
            timeType.CountsAsWorkTime,
            timeType.IsPaid,
            timeType.RequiresObject,
            timeType.IsAbsence,
            timeType.Color,
            timeType.SortOrder,
            timeType.IsActive,
            timeType.CreatedAtUtc,
            timeType.UpdatedAtUtc);
}
