using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Cleanifico.Contracts.Customers;
using Cleanifico.Domain.CleaningObjects;
using Cleanifico.Domain.Customers;

namespace Cleanifico.Api.Tests;

public sealed class CustomerEndpointTests
{
    private static readonly DateTime CreatedAt =
        new(2026, 8, 26, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task GetAll_SearchesRequiredFieldsAndFiltersStatus()
    {
        var berlin = CreateCustomer("K-100", "Nord GmbH", city: "Berlin");
        var contact = CreateCustomer("K-200", "Süd GmbH", firstName: "Erika", active: false);
        var other = CreateCustomer("K-300", "West GmbH", city: "Köln");
        await using var host = await ApiTestHost.StartWithCustomersAsync(berlin, contact, other);

        var byCity = await host.Client.GetFromJsonAsync<List<CustomerResponse>>(
            "/api/customers?search=Berlin&isActive=true");
        var byContact = await host.Client.GetFromJsonAsync<List<CustomerResponse>>(
            "/api/customers?search=Erika&isActive=false");

        Assert.Equal(["Nord GmbH"], byCity?.Select(customer => customer.CompanyName));
        Assert.Equal(["Süd GmbH"], byContact?.Select(customer => customer.CompanyName));
    }

    [Fact]
    public async Task GetById_ReturnsCompleteCustomerContract()
    {
        var existing = CreateCustomer("K-100", "Muster GmbH", city: "Berlin");
        await using var host = await ApiTestHost.StartWithCustomersAsync(existing);

        var result = await host.Client.GetFromJsonAsync<CustomerResponse>(
            $"/api/customers/{existing.Id}");

        Assert.NotNull(result);
        Assert.Equal(existing.Id, result.Id);
        Assert.Equal("K-100", result.CustomerNumber);
        Assert.Equal("Berlin", result.City);
    }

    [Fact]
    public async Task Create_ReturnsNormalizedCustomerAndLocation()
    {
        await using var host = await ApiTestHost.StartWithCustomersAsync();
        var request = Request(" K-100 ", " Muster GmbH ");

        using var response = await host.Client.PostAsJsonAsync("/api/customers", request);
        var result = await response.Content.ReadFromJsonAsync<CustomerResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal("K-100", result.CustomerNumber);
        Assert.Equal("Muster GmbH", result.CompanyName);
        Assert.Equal($"/api/customers/{result.Id}", response.Headers.Location?.OriginalString);
        Assert.Single(host.CustomerRepository.Items);
    }

    [Fact]
    public async Task Update_ChangesCustomerData()
    {
        var existing = CreateCustomer("K-100", "Alte Firma");
        await using var host = await ApiTestHost.StartWithCustomersAsync(existing);
        var request = Request("K-200", "Neue Firma");
        request.ContactFirstName = "Erika";
        request.City = "Berlin";

        using var response = await host.Client.PutAsJsonAsync(
            $"/api/customers/{existing.Id}",
            request);
        var result = await response.Content.ReadFromJsonAsync<CustomerResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal("K-200", result.CustomerNumber);
        Assert.Equal("Neue Firma", result.CompanyName);
        Assert.Equal("Erika", result.ContactFirstName);
        Assert.Equal("Berlin", result.City);
    }

    [Fact]
    public async Task ActivateDeactivateAndDelete_ChangeLifecycle()
    {
        var existing = CreateCustomer("K-100", "Muster GmbH");
        await using var host = await ApiTestHost.StartWithCustomersAsync(existing);

        using var deactivate = await host.Client.PostAsync(
            $"/api/customers/{existing.Id}/deactivate",
            null);
        Assert.Equal(HttpStatusCode.NoContent, deactivate.StatusCode);
        Assert.False(existing.IsActive);

        using var activate = await host.Client.PostAsync(
            $"/api/customers/{existing.Id}/activate",
            null);
        Assert.Equal(HttpStatusCode.NoContent, activate.StatusCode);
        Assert.True(existing.IsActive);

        using var delete = await host.Client.DeleteAsync($"/api/customers/{existing.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Empty(host.CustomerRepository.Items);
    }

    [Theory]
    [InlineData(" ", "Muster GmbH", "customerNumber")]
    [InlineData("K-100", " ", "companyName")]
    public async Task MissingRequiredField_ReturnsGermanValidationProblem(
        string customerNumber,
        string companyName,
        string expectedField)
    {
        await using var host = await ApiTestHost.StartWithCustomersAsync();
        var request = Request(customerNumber, companyName);

        using var response = await host.Client.PostAsJsonAsync("/api/customers", request);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(problem.RootElement.GetProperty("errors").TryGetProperty(expectedField, out _));
    }

    [Fact]
    public async Task DuplicateCustomerNumber_ReturnsConflict()
    {
        await using var host = await ApiTestHost.StartWithCustomersAsync(
            CreateCustomer("K-100", "Erste Firma"));

        using var response = await host.Client.PostAsJsonAsync(
            "/api/customers",
            Request("k-100", "Zweite Firma"));
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("customerNumber", problem.RootElement.GetProperty("field").GetString());
    }

    [Fact]
    public async Task Delete_CustomerWithObjectReturnsConflict_ButUnreferencedCustomerCanBeDeleted()
    {
        var referenced = CreateCustomer("K-100", "Referenziert GmbH");
        var unreferenced = CreateCustomer("K-200", "Frei GmbH");
        var cleaningObject = CleaningObject.Create(
            Guid.NewGuid(),
            new CleaningObjectData("O-1", referenced.Id, "Zentrale", null, null, null, null, null, null, null, null, null, null),
            CreatedAt);
        await using var host = await ApiTestHost.StartWithObjectsAsync([referenced, unreferenced], cleaningObject);

        using var blocked = await host.Client.DeleteAsync($"/api/customers/{referenced.Id}");
        using var deleted = await host.Client.DeleteAsync($"/api/customers/{unreferenced.Id}");

        Assert.Equal(HttpStatusCode.Conflict, blocked.StatusCode);
        Assert.Contains("mindestens ein Objekt", await blocked.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Contains(referenced, host.CustomerRepository.Items);
        Assert.DoesNotContain(unreferenced, host.CustomerRepository.Items);
    }

    private static UpdateCustomerRequest Request(string number, string company) => new()
    {
        CustomerNumber = number,
        CompanyName = company,
        ContactLastName = "Muster",
        Email = "kontakt@example.test",
        Phone = "+49 30 123",
        Street = "Musterstraße 1",
        PostalCode = "10115",
        City = "Berlin",
        Country = "Deutschland"
    };

    private static Customer CreateCustomer(
        string number,
        string company,
        string? city = null,
        string? firstName = null,
        bool active = true)
    {
        var customer = Customer.Create(
            Guid.NewGuid(),
            new CustomerData(
                number,
                company,
                firstName,
                "Muster",
                "kontakt@example.test",
                null,
                null,
                null,
                city,
                null,
                null),
            CreatedAt);
        if (!active)
        {
            customer.Deactivate(CreatedAt.AddHours(1));
        }

        return customer;
    }
}
