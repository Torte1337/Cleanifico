using Cleanifico.Domain.Common;
using Cleanifico.Domain.Customers;

namespace Cleanifico.Domain.Tests;

public sealed class CustomerTests
{
    private static readonly DateTime CreatedAt =
        new(2026, 8, 26, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_NormalizesDataAndStartsActive()
    {
        var customer = Customer.Create(
            Guid.NewGuid(),
            Data(
                number: " K-100 ",
                company: " Muster GmbH ",
                firstName: " Erika ",
                lastName: " Muster ",
                email: " erika@example.test ",
                city: " Berlin "),
            CreatedAt);

        Assert.Equal("K-100", customer.CustomerNumber);
        Assert.Equal("Muster GmbH", customer.CompanyName);
        Assert.Equal("Erika", customer.ContactFirstName);
        Assert.Equal("Muster", customer.ContactLastName);
        Assert.Equal("erika@example.test", customer.Email);
        Assert.Equal("Berlin", customer.City);
        Assert.True(customer.IsActive);
        Assert.Equal(CreatedAt, customer.CreatedAtUtc);
        Assert.Equal(CreatedAt, customer.UpdatedAtUtc);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_RejectsMissingCustomerNumber(string? customerNumber)
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            Customer.Create(Guid.NewGuid(), Data(number: customerNumber), CreatedAt));

        Assert.Contains("customerNumber", exception.Errors.Keys);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_RejectsMissingCompanyName(string? companyName)
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            Customer.Create(Guid.NewGuid(), Data(company: companyName), CreatedAt));

        Assert.Contains("companyName", exception.Errors.Keys);
    }

    [Fact]
    public void Create_RejectsInvalidOptionalEmail()
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            Customer.Create(Guid.NewGuid(), Data(email: "keine-email"), CreatedAt));

        Assert.Contains("email", exception.Errors.Keys);
    }

    [Fact]
    public void Update_ReplacesAllMutableFields()
    {
        var customer = Customer.Create(Guid.NewGuid(), Data(), CreatedAt);
        var updatedAt = CreatedAt.AddHours(1);

        customer.Update(
            new CustomerData(
                "K-200",
                "Neue Firma AG",
                "Nina",
                "Neu",
                "nina@example.test",
                "+49 30 123",
                "Neue Straße 2",
                "10115",
                "Berlin",
                "Deutschland",
                "Wichtiger Kunde"),
            updatedAt);

        Assert.Equal("K-200", customer.CustomerNumber);
        Assert.Equal("Neue Firma AG", customer.CompanyName);
        Assert.Equal("Nina", customer.ContactFirstName);
        Assert.Equal("Neu", customer.ContactLastName);
        Assert.Equal("nina@example.test", customer.Email);
        Assert.Equal("+49 30 123", customer.Phone);
        Assert.Equal("Neue Straße 2", customer.Street);
        Assert.Equal("10115", customer.PostalCode);
        Assert.Equal("Berlin", customer.City);
        Assert.Equal("Deutschland", customer.Country);
        Assert.Equal("Wichtiger Kunde", customer.Notes);
        Assert.Equal(updatedAt, customer.UpdatedAtUtc);
    }

    [Fact]
    public void ActivateAndDeactivate_ChangeStatus()
    {
        var customer = Customer.Create(Guid.NewGuid(), Data(), CreatedAt);

        customer.Deactivate(CreatedAt.AddMinutes(1));
        Assert.False(customer.IsActive);

        customer.Activate(CreatedAt.AddMinutes(2));
        Assert.True(customer.IsActive);
    }

    private static CustomerData Data(
        string? number = "K-100",
        string? company = "Muster GmbH",
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? city = null) =>
        new(
            number,
            company,
            firstName,
            lastName,
            email,
            null,
            null,
            null,
            city,
            null,
            null);
}
