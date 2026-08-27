using System.Net;
using System.Net.Http.Json;
using Cleanifico.Application.Licensing;
using Cleanifico.Contracts.EmployeeContracts;
using Cleanifico.Contracts.Security;
using Cleanifico.Domain.EmployeeContracts;
using Cleanifico.Domain.Employees;

namespace Cleanifico.Api.Tests;

public sealed class EmployeeContractEndpointTests
{
    private static readonly DateTime CreatedAt =
        new(2026, 8, 26, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Owner_CanCreateUpdateFilterLifecycleAndDeleteContract()
    {
        Employee employee = CreateEmployee("P-100", "Erika", "Muster");
        await using ApiTestHost host = await ApiTestHost.StartWithEmployeeContractsAsync([employee]);

        using HttpResponseMessage create = await host.Client.PostAsJsonAsync(
            "/api/employee-contracts",
            Request(employee.Id, " V-100 "));
        EmployeeContractResponse? created = await create.Content.ReadFromJsonAsync<EmployeeContractResponse>();
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        Assert.NotNull(created);
        Assert.Equal("V-100", created.ContractNumber);
        Assert.Equal("Erika Muster", created.EmployeeName);

        UpdateEmployeeContractRequest updateRequest = Request(employee.Id, "V-200");
        updateRequest.WeeklyHours = 30;
        using HttpResponseMessage update = await host.Client.PutAsJsonAsync(
            $"/api/employee-contracts/{created.Id}",
            updateRequest);
        EmployeeContractResponse? updated = await update.Content.ReadFromJsonAsync<EmployeeContractResponse>();
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal(30, updated?.WeeklyHours);

        using HttpResponseMessage filtered = await host.Client.GetAsync(
            $"/api/employee-contracts?search=Muster&isActive=true&employeeId={employee.Id}");
        List<EmployeeContractResponse>? found = await filtered.Content.ReadFromJsonAsync<List<EmployeeContractResponse>>();
        Assert.Single(found!);

        using HttpResponseMessage deactivate = await host.Client.PostAsync(
            $"/api/employee-contracts/{created.Id}/deactivate",
            null);
        Assert.Equal(HttpStatusCode.NoContent, deactivate.StatusCode);
        using HttpResponseMessage activate = await host.Client.PostAsync(
            $"/api/employee-contracts/{created.Id}/activate",
            null);
        Assert.Equal(HttpStatusCode.NoContent, activate.StatusCode);
        using HttpResponseMessage delete = await host.Client.DeleteAsync(
            $"/api/employee-contracts/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Empty(host.EmployeeContractRepository.Items);
    }

    [Fact]
    public async Task RequiredEmployeeUnknownEmployeeAndRequiredNumber_AreRejected()
    {
        Employee employee = CreateEmployee("P-100", "Erika", "Muster");
        await using ApiTestHost host = await ApiTestHost.StartWithEmployeeContractsAsync([employee]);

        using HttpResponseMessage missingEmployee = await host.Client.PostAsJsonAsync(
            "/api/employee-contracts",
            Request(Guid.Empty, "V-100"));
        using HttpResponseMessage unknownEmployee = await host.Client.PostAsJsonAsync(
            "/api/employee-contracts",
            Request(Guid.NewGuid(), "V-101"));
        using HttpResponseMessage missingNumber = await host.Client.PostAsJsonAsync(
            "/api/employee-contracts",
            Request(employee.Id, " "));

        Assert.Equal(HttpStatusCode.BadRequest, missingEmployee.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, unknownEmployee.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, missingNumber.StatusCode);
    }

    [Fact]
    public async Task DuplicateDatesNegativeValuesAndActiveOverlap_AreRejected()
    {
        Employee employee = CreateEmployee("P-100", "Erika", "Muster");
        EmployeeContract existing = CreateContract(
            employee.Id,
            "V-100",
            new(2026, 1, 1),
            new(2026, 12, 31));
        await using ApiTestHost host = await ApiTestHost.StartWithEmployeeContractsAsync(
            [employee],
            existing);

        using HttpResponseMessage duplicate = await host.Client.PostAsJsonAsync(
            "/api/employee-contracts",
            Request(employee.Id, "v-100", new(2027, 1, 1)));

        UpdateEmployeeContractRequest invalidDates = Request(
            employee.Id,
            "V-200",
            new(2026, 2, 1),
            new(2026, 1, 31),
            false);
        using HttpResponseMessage dates = await host.Client.PostAsJsonAsync(
            "/api/employee-contracts",
            invalidDates);

        UpdateEmployeeContractRequest negative = Request(employee.Id, "V-201", new(2027, 1, 1));
        negative.VacationDaysPerYear = -1;
        using HttpResponseMessage values = await host.Client.PostAsJsonAsync(
            "/api/employee-contracts",
            negative);

        using HttpResponseMessage overlap = await host.Client.PostAsJsonAsync(
            "/api/employee-contracts",
            Request(employee.Id, "V-202", new(2026, 6, 1)));

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, dates.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, values.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, overlap.StatusCode);
        Assert.Single(host.EmployeeContractRepository.Items);
    }

    [Fact]
    public async Task FollowUpContractPreservesHistoryAndEmployeeDeleteIsProtected()
    {
        Employee referenced = CreateEmployee("P-100", "Erika", "Muster");
        Employee unreferenced = CreateEmployee("P-200", "Paul", "Beispiel");
        EmployeeContract first = CreateContract(
            referenced.Id,
            "V-100",
            new(2025, 1, 1),
            new(2025, 12, 31));
        await using ApiTestHost host = await ApiTestHost.StartWithEmployeeContractsAsync(
            [referenced, unreferenced],
            first);

        using HttpResponseMessage next = await host.Client.PostAsJsonAsync(
            "/api/employee-contracts",
            Request(referenced.Id, "V-200", new(2026, 1, 1)));
        using HttpResponseMessage protectedDelete = await host.Client.DeleteAsync(
            $"/api/employees/{referenced.Id}");
        using HttpResponseMessage allowedDelete = await host.Client.DeleteAsync(
            $"/api/employees/{unreferenced.Id}");

        Assert.Equal(HttpStatusCode.Created, next.StatusCode);
        Assert.Equal(2, host.EmployeeContractRepository.Items.Count);
        Assert.Equal(HttpStatusCode.Conflict, protectedDelete.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, allowedDelete.StatusCode);
    }

    [Fact]
    public async Task AnonymousAndEmployeeCannotRead()
    {
        await using ApiTestHost anonymous = await ApiTestHost.StartAnonymousWithEmployeeContractsAsync([]);
        using HttpResponseMessage unauthorized = await anonymous.Client.GetAsync("/api/employee-contracts");
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);

        await using ApiTestHost employee = await ApiTestHost.StartAsRoleWithEmployeeContractsAsync(
            SecurityRoles.Employee,
            []);
        using HttpResponseMessage forbidden = await employee.Client.GetAsync("/api/employee-contracts");
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
    }

    [Theory]
    [InlineData(SecurityRoles.Dispatcher)]
    [InlineData(SecurityRoles.ObjectManager)]
    public async Task OfficeReaders_CanReadButCannotWrite(string role)
    {
        Employee employee = CreateEmployee("P-100", "Erika", "Muster");
        await using ApiTestHost host = await ApiTestHost.StartAsRoleWithEmployeeContractsAsync(
            role,
            [employee]);
        using HttpResponseMessage read = await host.Client.GetAsync("/api/employee-contracts");
        using HttpResponseMessage write = await host.Client.PostAsJsonAsync(
            "/api/employee-contracts",
            Request(employee.Id, "V-100"));

        Assert.Equal(HttpStatusCode.OK, read.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, write.StatusCode);
    }

    [Theory]
    [InlineData(SecurityRoles.Owner)]
    [InlineData(SecurityRoles.Administrator)]
    public async Task Administrators_CanManage(string role)
    {
        Employee employee = CreateEmployee("P-100", "Erika", "Muster");
        await using ApiTestHost host = await ApiTestHost.StartAsRoleWithEmployeeContractsAsync(
            role,
            [employee]);
        using HttpResponseMessage response = await host.Client.PostAsJsonAsync(
            "/api/employee-contracts",
            Request(employee.Id, "V-100"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task InvalidLicense_BlocksBusinessApi()
    {
        await using ApiTestHost host = await ApiTestHost.StartEmployeeContractsWithLicenseAsync(
            LicenseStatus.Expired);
        using HttpResponseMessage response = await host.Client.GetAsync("/api/employee-contracts");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static UpdateEmployeeContractRequest Request(
        Guid employeeId,
        string contractNumber,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        bool isPermanent = true) => new()
    {
        EmployeeId = employeeId,
        ContractNumber = contractNumber,
        StartDate = startDate ?? new DateOnly(2026, 1, 1),
        EndDate = endDate,
        IsPermanent = isPermanent,
        EmploymentType = "Vollzeit",
        WeeklyHours = 40,
        MonthlyTargetHours = 173,
        VacationDaysPerYear = 30
    };

    private static Employee CreateEmployee(string number, string firstName, string lastName) =>
        Employee.Create(
            Guid.NewGuid(),
            new EmployeeData(number, firstName, lastName, null, null, null, null, null, null, null, null, null),
            CreatedAt);

    private static EmployeeContract CreateContract(
        Guid employeeId,
        string number,
        DateOnly startDate,
        DateOnly? endDate) =>
        EmployeeContract.Create(
            Guid.NewGuid(),
            new EmployeeContractData(
                number,
                employeeId,
                startDate,
                endDate,
                !endDate.HasValue,
                "Vollzeit",
                40,
                173,
                30,
                null,
                null),
            CreatedAt);
}
