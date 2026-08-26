using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Cleanifico.Contracts.CleaningObjects;
using Cleanifico.Domain.CleaningObjects;
using Cleanifico.Domain.Customers;

namespace Cleanifico.Api.Tests;

public sealed class CleaningObjectEndpointTests
{
    private static readonly DateTime CreatedAt = new(2026, 8, 26, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task GetAll_SearchesCustomerAndFiltersStatusAndCustomer()
    {
        var north = Customer("K-1", "Nord GmbH"); var south = Customer("K-2", "Süd GmbH");
        var first = Item("O-1", north.Id, "Zentrale", "Berlin");
        var second = Item("O-2", south.Id, "Lager", "Hamburg"); second.Deactivate(CreatedAt.AddHours(1));
        await using var host = await ApiTestHost.StartWithObjectsAsync([north, south], first, second);

        var byCustomerName = await host.Client.GetFromJsonAsync<List<CleaningObjectResponse>>("/api/objects?search=Nord");
        var filtered = await host.Client.GetFromJsonAsync<List<CleaningObjectResponse>>($"/api/objects?isActive=false&customerId={south.Id}");

        Assert.Equal(["O-1"], byCustomerName?.Select(x => x.ObjectNumber));
        Assert.Equal(["O-2"], filtered?.Select(x => x.ObjectNumber));
    }

    [Fact]
    public async Task GetById_ReturnsObjectAndCustomerContract()
    {
        var customer = Customer("K-1", "Muster GmbH"); var item = Item("O-1", customer.Id, "Zentrale", "Berlin");
        await using var host = await ApiTestHost.StartWithObjectsAsync([customer], item);
        var result = await host.Client.GetFromJsonAsync<CleaningObjectResponse>($"/api/objects/{item.Id}");
        Assert.NotNull(result); Assert.Equal(customer.Id, result.CustomerId); Assert.Equal("K-1", result.CustomerNumber);
        Assert.Equal("Muster GmbH", result.CustomerCompanyName); Assert.Equal("Berlin", result.City);
    }

    [Fact]
    public async Task Create_NormalizesAndReturnsLocation()
    {
        var customer = Customer("K-1", "Muster GmbH"); await using var host = await ApiTestHost.StartWithObjectsAsync([customer]);
        using var response = await host.Client.PostAsJsonAsync("/api/objects", Request(" O-1 ", customer.Id, " Zentrale "));
        var result = await response.Content.ReadFromJsonAsync<CleaningObjectResponse>();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode); Assert.NotNull(result);
        Assert.Equal("O-1", result.ObjectNumber); Assert.Equal("Zentrale", result.Name);
        Assert.Equal($"/api/objects/{result.Id}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task UpdateAndLifecycleAndDelete_Work()
    {
        var customer = Customer("K-1", "Muster GmbH"); var item = Item("O-1", customer.Id, "Alt", null);
        await using var host = await ApiTestHost.StartWithObjectsAsync([customer], item);
        using var update = await host.Client.PutAsJsonAsync($"/api/objects/{item.Id}", Request("O-2", customer.Id, "Neu"));
        var changed = await update.Content.ReadFromJsonAsync<CleaningObjectResponse>();
        Assert.Equal("O-2", changed?.ObjectNumber); Assert.Equal("Neu", changed?.Name);
        Assert.Equal(HttpStatusCode.NoContent, (await host.Client.PostAsync($"/api/objects/{item.Id}/deactivate", null)).StatusCode); Assert.False(item.IsActive);
        Assert.Equal(HttpStatusCode.NoContent, (await host.Client.PostAsync($"/api/objects/{item.Id}/activate", null)).StatusCode); Assert.True(item.IsActive);
        Assert.Equal(HttpStatusCode.NoContent, (await host.Client.DeleteAsync($"/api/objects/{item.Id}")).StatusCode); Assert.Empty(host.CleaningObjectRepository.Items);
    }

    [Fact]
    public async Task MissingCustomer_ReturnsValidationProblem()
    {
        await using var host = await ApiTestHost.StartWithObjectsAsync([]);
        using var response = await host.Client.PostAsJsonAsync("/api/objects", Request("O-1", Guid.NewGuid(), "Zentrale"));
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(problem.RootElement.GetProperty("errors").TryGetProperty("customerId", out _));
    }

    [Fact]
    public async Task DuplicateObjectNumber_ReturnsConflict()
    {
        var customer = Customer("K-1", "Muster GmbH");
        await using var host = await ApiTestHost.StartWithObjectsAsync([customer], Item("O-1", customer.Id, "Alt", null));
        using var response = await host.Client.PostAsJsonAsync("/api/objects", Request("o-1", customer.Id, "Neu"));
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode); Assert.Equal("objectNumber", problem.RootElement.GetProperty("field").GetString());
    }

    internal static UpdateCleaningObjectRequest Request(string number, Guid customerId, string name) => new()
    { ObjectNumber = number, CustomerId = customerId, Name = name, Street = "Objektstraße 1", PostalCode = "10115", City = "Berlin", Country = "Deutschland", ContactLastName = "Muster", ContactEmail = "objekt@example.test" };

    internal static Customer Customer(string number, string company) => Cleanifico.Domain.Customers.Customer.Create(
        Guid.NewGuid(), new CustomerData(number, company, null, null, null, null, null, null, null, null, null), CreatedAt);
    internal static CleaningObject Item(string number, Guid customerId, string name, string? city) => CleaningObject.Create(
        Guid.NewGuid(), new CleaningObjectData(number, customerId, name, null, null, city, null, "Erika", "Muster", null, null, null, null), CreatedAt);
}
