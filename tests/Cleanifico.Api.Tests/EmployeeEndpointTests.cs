using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Cleanifico.Application.Licensing;
using Cleanifico.Contracts.Employees;
using Cleanifico.Contracts.Security;
using Cleanifico.Domain.Employees;

namespace Cleanifico.Api.Tests;

public sealed class EmployeeEndpointTests
{
    private static readonly DateTime CreatedAt =
        new(2026, 8, 26, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Owner_CanCreateUpdateSearchFilterAndDeleteEmployee()
    {
        await using ApiTestHost host = await ApiTestHost.StartWithEmployeesAsync();
        using HttpResponseMessage create = await host.Client.PostAsJsonAsync(
            "/api/employees",
            Request(" P-100 ", " Erika ", " Muster "));
        EmployeeResponse? created = await create.Content.ReadFromJsonAsync<EmployeeResponse>();
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        Assert.NotNull(created);
        Assert.Equal("P-100", created.EmployeeNumber);

        using HttpResponseMessage update = await host.Client.PutAsJsonAsync(
            $"/api/employees/{created.Id}",
            Request("P-200", "Nina", "Neu", city: "Berlin"));
        EmployeeResponse? updated = await update.Content.ReadFromJsonAsync<EmployeeResponse>();
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal("Nina", updated?.FirstName);

        using HttpResponseMessage search = await host.Client.GetAsync("/api/employees?search=Berlin&isActive=true");
        List<EmployeeResponse>? found = await search.Content.ReadFromJsonAsync<List<EmployeeResponse>>();
        Assert.Single(found!);

        using HttpResponseMessage deactivate = await host.Client.PostAsync($"/api/employees/{created.Id}/deactivate", null);
        Assert.Equal(HttpStatusCode.NoContent, deactivate.StatusCode);
        Assert.False(host.EmployeeRepository.Items.Single().IsActive);
        using HttpResponseMessage activate = await host.Client.PostAsync($"/api/employees/{created.Id}/activate", null);
        Assert.Equal(HttpStatusCode.NoContent, activate.StatusCode);

        using HttpResponseMessage delete = await host.Client.DeleteAsync($"/api/employees/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Empty(host.EmployeeRepository.Items);
    }

    [Theory]
    [InlineData(" ", "Erika", "Muster", "employeeNumber")]
    [InlineData("P-100", " ", "Muster", "firstName")]
    [InlineData("P-100", "Erika", " ", "lastName")]
    public async Task MissingRequiredField_ReturnsGermanValidationProblem(
        string number,
        string firstName,
        string lastName,
        string expectedField)
    {
        await using ApiTestHost host = await ApiTestHost.StartWithEmployeesAsync();
        using HttpResponseMessage response = await host.Client.PostAsJsonAsync(
            "/api/employees",
            Request(number, firstName, lastName));
        using JsonDocument problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(problem.RootElement.GetProperty("errors").TryGetProperty(expectedField, out _));
    }

    [Fact]
    public async Task DuplicateNumberInvalidDatesAndNegativeHours_AreRejected()
    {
        await using ApiTestHost host = await ApiTestHost.StartWithEmployeesAsync(
            CreateEmployee("P-100", "Erika", "Muster"));
        using HttpResponseMessage duplicate = await host.Client.PostAsJsonAsync(
            "/api/employees",
            Request("p-100", "Paul", "Beispiel"));
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        UpdateEmployeeRequest invalidDates = Request("P-200", "Paul", "Beispiel");
        invalidDates.EmploymentStartDate = new(2026, 8, 2);
        invalidDates.EmploymentEndDate = new(2026, 8, 1);
        using HttpResponseMessage dates = await host.Client.PostAsJsonAsync("/api/employees", invalidDates);
        Assert.Equal(HttpStatusCode.BadRequest, dates.StatusCode);

        UpdateEmployeeRequest invalidHours = Request("P-300", "Nina", "Neu");
        invalidHours.WeeklyHours = -1;
        using HttpResponseMessage hours = await host.Client.PostAsJsonAsync("/api/employees", invalidHours);
        Assert.Equal(HttpStatusCode.BadRequest, hours.StatusCode);
    }

    [Fact]
    public async Task AnonymousAndEmployeeCannotRead()
    {
        await using ApiTestHost anonymous = await ApiTestHost.StartAnonymousWithEmployeesAsync();
        using HttpResponseMessage unauthorized = await anonymous.Client.GetAsync("/api/employees");
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);

        await using ApiTestHost employee = await ApiTestHost.StartAsRoleWithEmployeesAsync(SecurityRoles.Employee);
        using HttpResponseMessage forbidden = await employee.Client.GetAsync("/api/employees");
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
    }

    [Theory]
    [InlineData(SecurityRoles.Dispatcher)]
    [InlineData(SecurityRoles.ObjectManager)]
    public async Task OfficeReaders_CanReadButCannotWrite(string role)
    {
        await using ApiTestHost host = await ApiTestHost.StartAsRoleWithEmployeesAsync(
            role,
            CreateEmployee("P-100", "Erika", "Muster"));
        using HttpResponseMessage read = await host.Client.GetAsync("/api/employees");
        using HttpResponseMessage write = await host.Client.PostAsJsonAsync(
            "/api/employees",
            Request("P-200", "Paul", "Beispiel"));
        Assert.Equal(HttpStatusCode.OK, read.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, write.StatusCode);
    }

    [Theory]
    [InlineData(SecurityRoles.Owner)]
    [InlineData(SecurityRoles.Administrator)]
    public async Task Administrators_CanManage(string role)
    {
        await using ApiTestHost host = await ApiTestHost.StartAsRoleWithEmployeesAsync(role);
        using HttpResponseMessage response = await host.Client.PostAsJsonAsync(
            "/api/employees",
            Request("P-100", "Erika", "Muster"));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task InvalidLicense_BlocksEmployeeBusinessApi()
    {
        await using ApiTestHost host = await ApiTestHost.StartEmployeesWithLicenseAsync(LicenseStatus.Expired);
        using HttpResponseMessage response = await host.Client.GetAsync("/api/employees");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static UpdateEmployeeRequest Request(
        string number,
        string firstName,
        string lastName,
        string? city = null) => new()
    {
        EmployeeNumber = number,
        FirstName = firstName,
        LastName = lastName,
        City = city,
        Email = "kontakt@example.test",
        EmploymentType = "Teilzeit",
        WeeklyHours = 20,
        MonthlyTargetHours = 86.5m
    };

    private static Employee CreateEmployee(string number, string firstName, string lastName) =>
        Employee.Create(
            Guid.NewGuid(),
            new EmployeeData(number, firstName, lastName, null, null, null, null, null, null, null, null, null, null, null, 0, 0, null),
            CreatedAt);
}
