using Cleanifico.Contracts.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Cleanifico.Infrastructure.Security.Authorization;

public static class SecurityAuthorizationExtensions
{
    public static IServiceCollection AddCleanificoApiAuthorization(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, ActiveUserAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, LicensedProductAuthorizationHandler>();

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new ActiveUserRequirement())
                .Build())
            .AddPolicy(SecurityPolicies.ActiveUser, policy =>
                policy.RequireAuthenticatedUser().AddRequirements(new ActiveUserRequirement()))
            .AddPolicy(SecurityPolicies.OfficeAccess, policy =>
                policy.RequireRole([.. SecurityRoles.Office]).AddRequirements(new ActiveUserRequirement()))
            .AddPolicy(SecurityPolicies.LicensedProduct, policy =>
                policy.RequireAuthenticatedUser().AddRequirements(
                    new ActiveUserRequirement(),
                    new LicensedProductRequirement()))
            .AddPolicy(SecurityPolicies.ViewCleaningTypes, policy =>
                policy.RequireRole([.. SecurityRoles.Office]).AddRequirements(
                    new ActiveUserRequirement(), new LicensedProductRequirement()))
            .AddPolicy(SecurityPolicies.ManageCleaningTypes, policy =>
                policy.RequireRole([.. SecurityRoles.Administrators]).AddRequirements(
                    new ActiveUserRequirement(), new LicensedProductRequirement()))
            .AddPolicy(SecurityPolicies.ViewTimeTypes, policy =>
                policy.RequireRole([.. SecurityRoles.Office]).AddRequirements(
                    new ActiveUserRequirement(), new LicensedProductRequirement()))
            .AddPolicy(SecurityPolicies.ManageTimeTypes, policy =>
                policy.RequireRole([.. SecurityRoles.Administrators]).AddRequirements(
                    new ActiveUserRequirement(), new LicensedProductRequirement()))
            .AddPolicy(SecurityPolicies.ViewCustomers, policy =>
                policy.RequireRole([.. SecurityRoles.Office]).AddRequirements(
                    new ActiveUserRequirement(), new LicensedProductRequirement()))
            .AddPolicy(SecurityPolicies.ManageCustomers, policy =>
                policy.RequireRole([.. SecurityRoles.Administrators]).AddRequirements(
                    new ActiveUserRequirement(), new LicensedProductRequirement()))
            .AddPolicy(SecurityPolicies.ViewObjects, policy =>
                policy.RequireRole([.. SecurityRoles.Office]).AddRequirements(
                    new ActiveUserRequirement(), new LicensedProductRequirement()))
            .AddPolicy(SecurityPolicies.ManageObjects, policy =>
                policy.RequireRole([.. SecurityRoles.Administrators]).AddRequirements(
                    new ActiveUserRequirement(), new LicensedProductRequirement()))
            .AddPolicy(SecurityPolicies.ViewEmployees, policy =>
                policy.RequireRole([.. SecurityRoles.Office]).AddRequirements(
                    new ActiveUserRequirement(), new LicensedProductRequirement()))
            .AddPolicy(SecurityPolicies.ManageEmployees, policy =>
                policy.RequireRole([.. SecurityRoles.Administrators]).AddRequirements(
                    new ActiveUserRequirement(), new LicensedProductRequirement()))
            .AddPolicy(SecurityPolicies.ViewEmployeeContracts, policy =>
                policy.RequireRole([.. SecurityRoles.Office]).AddRequirements(
                    new ActiveUserRequirement(), new LicensedProductRequirement()))
            .AddPolicy(SecurityPolicies.ManageEmployeeContracts, policy =>
                policy.RequireRole([.. SecurityRoles.Administrators]).AddRequirements(
                    new ActiveUserRequirement(), new LicensedProductRequirement()))
            .AddPolicy(SecurityPolicies.ManageUsers, policy =>
                policy.RequireRole([.. SecurityRoles.Administrators]).AddRequirements(new ActiveUserRequirement()))
            .AddPolicy(SecurityPolicies.ManageRoles, policy =>
                policy.RequireRole([.. SecurityRoles.Administrators]).AddRequirements(new ActiveUserRequirement()))
            .AddPolicy(SecurityPolicies.AdministrationAccess, policy =>
                policy.RequireRole([.. SecurityRoles.Administrators]).AddRequirements(new ActiveUserRequirement()))
            .AddPolicy(SecurityPolicies.ManageLicense, policy =>
                policy.RequireRole([.. SecurityRoles.Administrators]).AddRequirements(new ActiveUserRequirement()));

        return services;
    }
}
