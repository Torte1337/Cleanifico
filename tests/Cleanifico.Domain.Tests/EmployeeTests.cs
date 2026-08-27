using Cleanifico.Domain.Common;
using Cleanifico.Domain.Employees;

namespace Cleanifico.Domain.Tests;

public sealed class EmployeeTests
{
    private static readonly DateTime CreatedAt =
        new(2026, 8, 26, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_NormalizesDataAndStartsActive()
    {
        Employee employee = Employee.Create(
            Guid.NewGuid(),
            Data(number: " P-100 ", firstName: " Erika ", lastName: " Muster ", email: " erika@example.test "),
            CreatedAt);

        Assert.Equal("P-100", employee.EmployeeNumber);
        Assert.Equal("Erika", employee.FirstName);
        Assert.Equal("Muster", employee.LastName);
        Assert.Equal("erika@example.test", employee.Email);
        Assert.True(employee.IsActive);
        Assert.Equal(CreatedAt, employee.CreatedAtUtc);
    }

    [Theory]
    [InlineData(null, "Erika", "Muster", "employeeNumber")]
    [InlineData("P-100", " ", "Muster", "firstName")]
    [InlineData("P-100", "Erika", "", "lastName")]
    public void Create_RejectsMissingRequiredFields(
        string? number,
        string? firstName,
        string? lastName,
        string expectedField)
    {
        DomainValidationException exception = Assert.Throws<DomainValidationException>(() =>
            Employee.Create(Guid.NewGuid(), Data(number, firstName, lastName), CreatedAt));

        Assert.Contains(expectedField, exception.Errors.Keys);
    }

    [Fact]
    public void UpdateAndLifecycle_ChangeMutablePersonalState()
    {
        Employee employee = Employee.Create(Guid.NewGuid(), Data(), CreatedAt);
        employee.Update(Data(number: "P-200", firstName: "Nina", city: "Berlin"), CreatedAt.AddHours(1));
        employee.Deactivate(CreatedAt.AddHours(2));

        Assert.Equal("P-200", employee.EmployeeNumber);
        Assert.Equal("Nina", employee.FirstName);
        Assert.Equal("Berlin", employee.City);
        Assert.False(employee.IsActive);

        employee.Activate(CreatedAt.AddHours(3));
        Assert.True(employee.IsActive);
    }

    private static EmployeeData Data(
        string? number = "P-100",
        string? firstName = "Erika",
        string? lastName = "Muster",
        string? email = null,
        string? city = null) =>
        new(number, firstName, lastName, null, null, city, null, email, null, null, null, null);
}
