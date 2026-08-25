using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Cleanifico.Contracts.CleaningTypes;
using Cleanifico.Domain.CleaningTypes;

namespace Cleanifico.Api.Tests;

public sealed class CleaningTypeEndpointTests
{
    private static readonly DateTime CreatedAt = new(2026, 8, 25, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task GetAll_FiltersBySearchAndStatusAndUsesDefaultSort()
    {
        var later = CreateType("Unterhaltsreinigung", "UR", 20);
        var earlier = CreateType("Glasreinigung", "GL", 10);
        var inactive = CreateType("Inaktive Reinigung", "IR", 1, active: false);
        await using var host = await ApiTestHost.StartAsync(later, inactive, earlier);

        var result = await host.Client.GetFromJsonAsync<List<CleaningTypeResponse>>(
            "/api/cleaning-types?search=reinigung&isActive=true");

        Assert.NotNull(result);
        Assert.Equal(["Glasreinigung", "Unterhaltsreinigung"], result.Select(item => item.Name));
    }

    [Fact]
    public async Task GetById_ReturnsCleaningTypeContract()
    {
        var existing = CreateType();
        await using var host = await ApiTestHost.StartAsync(existing);

        var response = await host.Client.GetFromJsonAsync<CleaningTypeResponse>(
            $"/api/cleaning-types/{existing.Id}");

        Assert.NotNull(response);
        Assert.Equal(existing.Id, response.Id);
        Assert.Equal("Unterhaltsreinigung", response.Name);
    }

    [Fact]
    public async Task Create_ReturnsCreatedContractAndLocation()
    {
        await using var host = await ApiTestHost.StartAsync();
        var request = new CreateCleaningTypeRequest
        {
            Name = " Grundreinigung ",
            Code = " gr ",
            Description = "Intensiv",
            SortOrder = 30
        };

        using var response = await host.Client.PostAsJsonAsync("/api/cleaning-types", request);
        var result = await response.Content.ReadFromJsonAsync<CleaningTypeResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal("Grundreinigung", result.Name);
        Assert.Equal("GR", result.Code);
        Assert.Equal($"/api/cleaning-types/{result.Id}", response.Headers.Location?.OriginalString);
        Assert.Single(host.Repository.Items);
    }

    [Fact]
    public async Task Update_ChangesMutableFields()
    {
        var existing = CreateType();
        await using var host = await ApiTestHost.StartAsync(existing);
        var request = new UpdateCleaningTypeRequest
        {
            Name = "Sonderreinigung",
            Code = "SR",
            Description = "Nach Bedarf",
            SortOrder = 40
        };

        using var response = await host.Client.PutAsJsonAsync(
            $"/api/cleaning-types/{existing.Id}",
            request);
        var result = await response.Content.ReadFromJsonAsync<CleaningTypeResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal("Sonderreinigung", result.Name);
        Assert.Equal("SR", result.Code);
        Assert.Equal(40, result.SortOrder);
    }

    [Fact]
    public async Task DeactivateAndActivate_ChangeLifecycleStatus()
    {
        var existing = CreateType();
        await using var host = await ApiTestHost.StartAsync(existing);

        using var deactivateResponse = await host.Client.PostAsync(
            $"/api/cleaning-types/{existing.Id}/deactivate",
            null);
        Assert.Equal(HttpStatusCode.NoContent, deactivateResponse.StatusCode);
        Assert.False(existing.IsActive);

        using var activateResponse = await host.Client.PostAsync(
            $"/api/cleaning-types/{existing.Id}/activate",
            null);
        Assert.Equal(HttpStatusCode.NoContent, activateResponse.StatusCode);
        Assert.True(existing.IsActive);
    }

    [Fact]
    public async Task Delete_RemovesCleaningType()
    {
        var existing = CreateType();
        await using var host = await ApiTestHost.StartAsync(existing);

        using var response = await host.Client.DeleteAsync($"/api/cleaning-types/{existing.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(host.Repository.Items);
    }

    [Fact]
    public async Task MissingCleaningType_ReturnsNotFoundProblem()
    {
        await using var host = await ApiTestHost.StartAsync();

        using var response = await host.Client.GetAsync($"/api/cleaning-types/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task InvalidRequest_ReturnsValidationProblem()
    {
        await using var host = await ApiTestHost.StartAsync();
        var request = new CreateCleaningTypeRequest { Name = " ", Code = "UR", SortOrder = 0 };

        using var response = await host.Client.PostAsJsonAsync("/api/cleaning-types", request);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(problem.RootElement.GetProperty("errors").TryGetProperty("name", out _));
    }

    [Fact]
    public async Task DuplicateCode_ReturnsConflictProblemWithField()
    {
        await using var host = await ApiTestHost.StartAsync(CreateType(code: "UR"));
        var request = new CreateCleaningTypeRequest
        {
            Name = "Andere Reinigung",
            Code = "ur",
            SortOrder = 0
        };

        using var response = await host.Client.PostAsJsonAsync("/api/cleaning-types", request);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("code", problem.RootElement.GetProperty("field").GetString());
    }

    private static CleaningType CreateType(
        string name = "Unterhaltsreinigung",
        string code = "UR",
        int sortOrder = 10,
        bool active = true)
    {
        var cleaningType = CleaningType.Create(
            Guid.NewGuid(),
            name,
            code,
            null,
            sortOrder,
            CreatedAt);

        if (!active)
        {
            cleaningType.Deactivate(CreatedAt.AddHours(1));
        }

        return cleaningType;
    }
}
