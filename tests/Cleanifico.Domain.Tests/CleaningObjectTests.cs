using Cleanifico.Domain.CleaningObjects;
using Cleanifico.Domain.Common;

namespace Cleanifico.Domain.Tests;

public sealed class CleaningObjectTests
{
    private static readonly Guid CustomerId = Guid.NewGuid();
    private static readonly DateTime CreatedAt = new(2026, 8, 26, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_NormalizesRequiredAndOptionalFields()
    {
        var item = CleaningObject.Create(Guid.NewGuid(), Data(" OBJ-1 ", CustomerId, " Zentrale ") with
        {
            City = " Berlin ", ContactEmail = " objekt@example.test "
        }, CreatedAt);

        Assert.Equal("OBJ-1", item.ObjectNumber);
        Assert.Equal("Zentrale", item.Name);
        Assert.Equal("Berlin", item.City);
        Assert.Equal("objekt@example.test", item.ContactEmail);
        Assert.True(item.IsActive);
    }

    [Theory]
    [InlineData(null, "Name")]
    [InlineData("OBJ-1", null)]
    public void Create_RejectsMissingRequiredText(string? number, string? name)
    {
        Assert.Throws<DomainValidationException>(() =>
            CleaningObject.Create(Guid.NewGuid(), Data(number, CustomerId, name), CreatedAt));
    }

    [Fact]
    public void Create_RejectsMissingCustomer()
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            CleaningObject.Create(Guid.NewGuid(), Data("OBJ-1", Guid.Empty, "Zentrale"), CreatedAt));
        Assert.Contains("customerId", exception.Errors.Keys);
    }

    [Fact]
    public void Update_AllowsNumberCustomerAndAddressChanges()
    {
        var item = CleaningObject.Create(Guid.NewGuid(), Data("OBJ-1", CustomerId, "Alt"), CreatedAt);
        var newCustomerId = Guid.NewGuid();
        item.Update(Data("OBJ-2", newCustomerId, "Neu") with { City = "Hamburg" }, CreatedAt.AddHours(1));

        Assert.Equal("OBJ-2", item.ObjectNumber);
        Assert.Equal(newCustomerId, item.CustomerId);
        Assert.Equal("Hamburg", item.City);
    }

    [Fact]
    public void Lifecycle_IsIdempotentAndUpdatesUtcAudit()
    {
        var item = CleaningObject.Create(Guid.NewGuid(), Data("OBJ-1", CustomerId, "Zentrale"), CreatedAt);
        item.Deactivate(CreatedAt.AddHours(1));
        item.Deactivate(CreatedAt.AddHours(2));
        Assert.False(item.IsActive);
        Assert.Equal(CreatedAt.AddHours(1), item.UpdatedAtUtc);
        item.Activate(CreatedAt.AddHours(3));
        Assert.True(item.IsActive);
        Assert.Equal(CreatedAt.AddHours(3), item.UpdatedAtUtc);
    }

    [Fact]
    public void Create_RejectsInvalidEmailAndNonUtcAudit()
    {
        Assert.Throws<DomainValidationException>(() => CleaningObject.Create(
            Guid.NewGuid(), Data("OBJ-1", CustomerId, "Zentrale") with { ContactEmail = "falsch" }, CreatedAt));
        Assert.Throws<DomainValidationException>(() => CleaningObject.Create(
            Guid.NewGuid(), Data("OBJ-1", CustomerId, "Zentrale"), DateTime.SpecifyKind(CreatedAt, DateTimeKind.Local)));
    }

    private static CleaningObjectData Data(string? number, Guid customerId, string? name) =>
        new(number, customerId, name, null, null, null, null, null, null, null, null, null, null);
}
