using System.Net;
using System.Net.Http.Json;
using Cleanifico.Contracts.CleaningTypes;
using Cleanifico.Contracts.Security;
using Cleanifico.Contracts.TimeTypes;
using Cleanifico.Domain.CleaningTypes;

namespace Cleanifico.Api.Tests;

public sealed class AuthorizationEndpointTests
{
    [Theory]
    [InlineData(HttpMethodName.Get)]
    [InlineData(HttpMethodName.Post)]
    public async Task AnonymousCleaningTypeRequest_ReturnsUnauthorized(HttpMethodName method)
    {
        await using var host = await ApiTestHost.StartAnonymousAsync();
        using var request = new HttpRequestMessage(
            method == HttpMethodName.Get ? HttpMethod.Get : HttpMethod.Post,
            "/api/cleaning-types");

        if (method == HttpMethodName.Post)
        {
            request.Content = JsonContent.Create(CreateRequest());
        }

        using var response = await host.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData(SecurityRoles.Owner)]
    [InlineData(SecurityRoles.Administrator)]
    public async Task AdministratorRoles_CanReadAndCreateCleaningTypes(string role)
    {
        await using var host = await ApiTestHost.StartAsRoleAsync(role);

        using var getResponse = await host.Client.GetAsync("/api/cleaning-types");
        using var postResponse = await host.Client.PostAsJsonAsync("/api/cleaning-types", CreateRequest());

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
    }

    [Theory]
    [InlineData(SecurityRoles.Dispatcher)]
    [InlineData(SecurityRoles.ObjectManager)]
    public async Task ReadOnlyOfficeRoles_CanReadButCannotCreate(string role)
    {
        await using var host = await ApiTestHost.StartAsRoleAsync(role);

        using var getResponse = await host.Client.GetAsync("/api/cleaning-types");
        using var postResponse = await host.Client.PostAsJsonAsync("/api/cleaning-types", CreateRequest());

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, postResponse.StatusCode);
    }

    [Fact]
    public async Task Dispatcher_CannotUpdateOrDeleteCleaningTypes()
    {
        var existing = CreateCleaningType();
        await using var host = await ApiTestHost.StartAsRoleAsync(SecurityRoles.Dispatcher, existing);

        using var putResponse = await host.Client.PutAsJsonAsync(
            $"/api/cleaning-types/{existing.Id}",
            new UpdateCleaningTypeRequest { Name = "Neu", Code = "NEU", SortOrder = 0 });
        using var deleteResponse = await host.Client.DeleteAsync($"/api/cleaning-types/{existing.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, putResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Employee_CannotUseCleaningTypeOrUserAdministrationApis()
    {
        await using var host = await ApiTestHost.StartAsRoleAsync(SecurityRoles.Employee);

        using var cleaningTypesResponse = await host.Client.GetAsync("/api/cleaning-types");
        using var usersResponse = await host.Client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Forbidden, cleaningTypesResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, usersResponse.StatusCode);
    }

    [Theory]
    [InlineData(SecurityRoles.Owner, HttpStatusCode.OK)]
    [InlineData(SecurityRoles.Administrator, HttpStatusCode.OK)]
    [InlineData(SecurityRoles.Dispatcher, HttpStatusCode.Forbidden)]
    [InlineData(SecurityRoles.ObjectManager, HttpStatusCode.Forbidden)]
    public async Task UserAdministration_RequiresManageUsersPolicy(string role, HttpStatusCode expected)
    {
        await using var host = await ApiTestHost.StartAsRoleAsync(role);

        using var response = await host.Client.GetAsync("/api/users");

        Assert.Equal(expected, response.StatusCode);
    }

    [Fact]
    public async Task AnonymousTimeTypeRequest_ReturnsUnauthorized()
    {
        await using var host = await ApiTestHost.StartAnonymousWithTimeTypesAsync();

        using var getResponse = await host.Client.GetAsync("/api/time-types");
        using var postResponse = await host.Client.PostAsJsonAsync("/api/time-types", CreateTimeTypeRequest());

        Assert.Equal(HttpStatusCode.Unauthorized, getResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, postResponse.StatusCode);
    }

    [Theory]
    [InlineData(SecurityRoles.Owner)]
    [InlineData(SecurityRoles.Administrator)]
    public async Task AdministratorRoles_CanReadAndManageTimeTypes(string role)
    {
        await using var host = await ApiTestHost.StartAsRoleWithTimeTypesAsync(role);

        using var getResponse = await host.Client.GetAsync("/api/time-types");
        using var postResponse = await host.Client.PostAsJsonAsync("/api/time-types", CreateTimeTypeRequest());

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
    }

    [Theory]
    [InlineData(SecurityRoles.Dispatcher)]
    [InlineData(SecurityRoles.ObjectManager)]
    public async Task ReadOnlyOfficeRoles_CanReadButCannotManageTimeTypes(string role)
    {
        await using var host = await ApiTestHost.StartAsRoleWithTimeTypesAsync(role);

        using var getResponse = await host.Client.GetAsync("/api/time-types");
        using var postResponse = await host.Client.PostAsJsonAsync("/api/time-types", CreateTimeTypeRequest());

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, postResponse.StatusCode);
    }

    [Fact]
    public async Task Employee_CannotUseTimeTypeAdministrationApi()
    {
        await using var host = await ApiTestHost.StartAsRoleWithTimeTypesAsync(SecurityRoles.Employee);

        using var response = await host.Client.GetAsync("/api/time-types");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static CreateCleaningTypeRequest CreateRequest() => new()
    {
        Name = "Sicherheitsreinigung",
        Code = "SEC",
        SortOrder = 10
    };

    private static CleaningType CreateCleaningType() => CleaningType.Create(
        Guid.NewGuid(),
        "Unterhaltsreinigung",
        "UR",
        null,
        10,
        new DateTime(2026, 8, 25, 8, 0, 0, DateTimeKind.Utc));

    private static CreateTimeTypeRequest CreateTimeTypeRequest() => new()
    {
        Name = "Arbeitszeit",
        Code = "ARB",
        CountsAsWorkTime = true,
        IsPaid = true,
        RequiresObject = true,
        Color = "#2F855A",
        SortOrder = 10
    };

    public enum HttpMethodName
    {
        Get,
        Post
    }
}
