using Cleanifico.Application.Security;
using Cleanifico.Contracts.Security;
using Cleanifico.Contracts.Users;

namespace Cleanifico.Api.Endpoints;

public static class UserAdministrationEndpoints
{
    public static IEndpointRouteBuilder MapUserAdministrationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var users = endpoints.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization(SecurityPolicies.ManageUsers);

        users.MapGet("", GetAllAsync);
        users.MapGet("/{id:guid}", GetByIdAsync);
        users.MapPost("", CreateAsync);
        users.MapPut("/{id:guid}", UpdateAsync);
        users.MapPost("/{id:guid}/activate", ActivateAsync);
        users.MapPost("/{id:guid}/deactivate", DeactivateAsync);
        users.MapPut("/{id:guid}/roles", UpdateRolesAsync)
            .RequireAuthorization(SecurityPolicies.ManageRoles);

        endpoints.MapGet("/api/roles", GetRolesAsync)
            .WithTags("Users")
            .RequireAuthorization(SecurityPolicies.ManageRoles);

        return endpoints;
    }

    private static async Task<IResult> GetAllAsync(
        IUserAdministrationService service,
        CancellationToken cancellationToken) =>
        Results.Ok((await service.GetAllAsync(cancellationToken)).Select(ToResponse));

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        IUserAdministrationService service,
        CancellationToken cancellationToken) =>
        Results.Ok(ToResponse(await service.GetByIdAsync(id, cancellationToken)));

    private static async Task<IResult> CreateAsync(
        CreateUserRequest request,
        IUserAdministrationService service,
        CancellationToken cancellationToken)
    {
        var user = await service.CreateAsync(
            new CreateUserInput(
                request.FirstName,
                request.LastName,
                request.Email,
                request.Password,
                request.Roles,
                request.IsActive),
            cancellationToken);

        return Results.Created($"/api/users/{user.Id}", ToResponse(user));
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateUserRequest request,
        IUserAdministrationService service,
        CancellationToken cancellationToken)
    {
        var user = await service.UpdateAsync(
            id,
            new UpdateUserInput(request.FirstName, request.LastName, request.Email, request.IsActive),
            cancellationToken);

        return Results.Ok(ToResponse(user));
    }

    private static async Task<IResult> ActivateAsync(
        Guid id,
        IUserAdministrationService service,
        CancellationToken cancellationToken)
    {
        await service.ActivateAsync(id, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> DeactivateAsync(
        Guid id,
        IUserAdministrationService service,
        CancellationToken cancellationToken)
    {
        await service.DeactivateAsync(id, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> UpdateRolesAsync(
        Guid id,
        UpdateUserRolesRequest request,
        IUserAdministrationService service,
        CancellationToken cancellationToken) =>
        Results.Ok(ToResponse(await service.UpdateRolesAsync(id, request.Roles, cancellationToken)));

    private static async Task<IResult> GetRolesAsync(
        IUserAdministrationService service,
        CancellationToken cancellationToken) =>
        Results.Ok((await service.GetRolesAsync(cancellationToken))
            .Select(role => new RoleResponse(role, SecurityRoles.GetDisplayName(role))));

    private static UserResponse ToResponse(UserAccount user) =>
        new(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.IsActive,
            user.Roles,
            user.CreatedAtUtc,
            user.UpdatedAtUtc);
}
