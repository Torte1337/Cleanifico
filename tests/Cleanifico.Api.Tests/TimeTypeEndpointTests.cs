using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Cleanifico.Contracts.TimeTypes;
using Cleanifico.Domain.TimeTypes;

namespace Cleanifico.Api.Tests;

public sealed class TimeTypeEndpointTests
{
    private static readonly DateTime CreatedAt =
        new(2026, 8, 25, 18, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task GetAll_FiltersBySearchAndStatusAndSorts()
    {
        var work = CreateType("Arbeitszeit", "ARB", 20);
        var trip = CreateType("Fahrzeit", "FAH", 10);
        var inactive = CreateType("Freizeit", "FRE", 1, active: false);
        await using var host = await ApiTestHost.StartWithTimeTypesAsync(work, inactive, trip);

        var result = await host.Client.GetFromJsonAsync<List<TimeTypeResponse>>(
            "/api/time-types?search=zeit&isActive=true");

        Assert.NotNull(result);
        Assert.Equal(["Fahrzeit", "Arbeitszeit"], result.Select(item => item.Name));
    }

    [Fact]
    public async Task Create_ReturnsNormalizedContractWithAllProperties()
    {
        await using var host = await ApiTestHost.StartWithTimeTypesAsync();
        var request = new CreateTimeTypeRequest
        {
            Name = " Schulung ",
            Code = " sch ",
            Description = "Fortbildung",
            CountsAsWorkTime = true,
            IsPaid = true,
            RequiresObject = false,
            IsAbsence = false,
            Color = "#319795",
            SortOrder = -2
        };

        using var response = await host.Client.PostAsJsonAsync("/api/time-types", request);
        var result = await response.Content.ReadFromJsonAsync<TimeTypeResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal("Schulung", result.Name);
        Assert.Equal("SCH", result.Code);
        Assert.True(result.CountsAsWorkTime);
        Assert.True(result.IsPaid);
        Assert.Equal(-2, result.SortOrder);
        Assert.Equal($"/api/time-types/{result.Id}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Update_ChangesEveryConfigurableProperty()
    {
        var existing = CreateType("Arbeitszeit", "ARB", 10);
        await using var host = await ApiTestHost.StartWithTimeTypesAsync(existing);
        var request = new UpdateTimeTypeRequest
        {
            Name = "Urlaub",
            Code = "URL",
            Description = "Abwesenheit",
            CountsAsWorkTime = false,
            IsPaid = false,
            RequiresObject = false,
            IsAbsence = true,
            Color = "#805AD5",
            SortOrder = 99
        };

        using var response = await host.Client.PutAsJsonAsync($"/api/time-types/{existing.Id}", request);
        var result = await response.Content.ReadFromJsonAsync<TimeTypeResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal("Urlaub", result.Name);
        Assert.Equal("URL", result.Code);
        Assert.False(result.CountsAsWorkTime);
        Assert.False(result.IsPaid);
        Assert.True(result.IsAbsence);
        Assert.Equal("#805AD5", result.Color);
        Assert.Equal(99, result.SortOrder);
    }

    [Fact]
    public async Task ActivateDeactivateAndDelete_ChangeLifecycle()
    {
        var existing = CreateType("Arbeitszeit", "ARB", 10);
        await using var host = await ApiTestHost.StartWithTimeTypesAsync(existing);

        using var deactivate = await host.Client.PostAsync(
            $"/api/time-types/{existing.Id}/deactivate",
            null);
        Assert.Equal(HttpStatusCode.NoContent, deactivate.StatusCode);
        Assert.False(existing.IsActive);

        using var activate = await host.Client.PostAsync(
            $"/api/time-types/{existing.Id}/activate",
            null);
        Assert.Equal(HttpStatusCode.NoContent, activate.StatusCode);
        Assert.True(existing.IsActive);

        using var delete = await host.Client.DeleteAsync($"/api/time-types/{existing.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Empty(host.TimeTypeRepository.Items);
    }

    [Fact]
    public async Task MissingRequiredName_ReturnsValidationProblem()
    {
        await using var host = await ApiTestHost.StartWithTimeTypesAsync();
        var request = new CreateTimeTypeRequest { Name = " ", Code = "ARB" };

        using var response = await host.Client.PostAsJsonAsync("/api/time-types", request);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(problem.RootElement.GetProperty("errors").TryGetProperty("name", out _));
    }

    private static TimeType CreateType(
        string name,
        string code,
        int sortOrder,
        bool active = true)
    {
        var timeType = TimeType.Create(
            Guid.NewGuid(),
            name,
            code,
            null,
            true,
            true,
            false,
            false,
            "#2F855A",
            sortOrder,
            CreatedAt);

        if (!active)
        {
            timeType.Deactivate(CreatedAt.AddHours(1));
        }

        return timeType;
    }
}
