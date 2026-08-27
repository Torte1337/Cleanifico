using Cleanifico.Domain.Common;
using Cleanifico.Domain.EmployeeContracts;

namespace Cleanifico.Domain.Tests;

public sealed class EmployeeContractTests
{
    private static readonly DateTime CreatedAt =
        new(2026, 8, 26, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_NormalizesDataAndStartsActive()
    {
        EmployeeContract contract = EmployeeContract.Create(
            Guid.NewGuid(),
            Data(contractNumber: " V-100 ", employmentType: " Teilzeit "),
            CreatedAt);

        Assert.Equal("V-100", contract.ContractNumber);
        Assert.Equal("Teilzeit", contract.EmploymentType);
        Assert.True(contract.IsActive);
        Assert.Equal(CreatedAt, contract.CreatedAtUtc);
    }

    [Theory]
    [InlineData("contractNumber")]
    [InlineData("employeeId")]
    [InlineData("startDate")]
    public void Create_RejectsMissingRequiredFields(string field)
    {
        EmployeeContractData data = field switch
        {
            "contractNumber" => Data(contractNumber: " "),
            "employeeId" => Data(employeeId: Guid.Empty),
            _ => Data(startDate: DateOnly.MinValue)
        };

        DomainValidationException exception = Assert.Throws<DomainValidationException>(() =>
            EmployeeContract.Create(Guid.NewGuid(), data, CreatedAt));

        Assert.Contains(field, exception.Errors.Keys);
    }

    [Theory]
    [InlineData("endDate")]
    [InlineData("probationEndDate")]
    public void Create_RejectsDateBeforeStart(string field)
    {
        EmployeeContractData data = field == "endDate"
            ? Data(endDate: new(2026, 1, 31), isPermanent: false)
            : Data(probationEndDate: new(2026, 1, 31));

        DomainValidationException exception = Assert.Throws<DomainValidationException>(() =>
            EmployeeContract.Create(Guid.NewGuid(), data, CreatedAt));

        Assert.Contains(field, exception.Errors.Keys);
    }

    [Fact]
    public void Create_RejectsEndDateForPermanentContract()
    {
        DomainValidationException exception = Assert.Throws<DomainValidationException>(() =>
            EmployeeContract.Create(
                Guid.NewGuid(),
                Data(endDate: new(2026, 12, 31), isPermanent: true),
                CreatedAt));

        Assert.Contains("endDate", exception.Errors.Keys);
    }

    [Theory]
    [InlineData(-1, 0, 0, "weeklyHours")]
    [InlineData(0, -1, 0, "monthlyTargetHours")]
    [InlineData(0, 0, -1, "vacationDaysPerYear")]
    public void Create_RejectsNegativeConditions(
        decimal weeklyHours,
        decimal monthlyHours,
        decimal vacationDays,
        string field)
    {
        DomainValidationException exception = Assert.Throws<DomainValidationException>(() =>
            EmployeeContract.Create(
                Guid.NewGuid(),
                Data(
                    weeklyHours: weeklyHours,
                    monthlyHours: monthlyHours,
                    vacationDays: vacationDays),
                CreatedAt));

        Assert.Contains(field, exception.Errors.Keys);
    }

    [Fact]
    public void UpdateAndLifecycle_KeepIdentityAndHistoryRecord()
    {
        EmployeeContract contract = EmployeeContract.Create(Guid.NewGuid(), Data(), CreatedAt);
        contract.Update(Data(contractNumber: "V-200", weeklyHours: 30), CreatedAt.AddHours(1));
        contract.Deactivate(CreatedAt.AddHours(2));

        Assert.Equal("V-200", contract.ContractNumber);
        Assert.Equal(30, contract.WeeklyHours);
        Assert.False(contract.IsActive);

        contract.Activate(CreatedAt.AddHours(3));
        Assert.True(contract.IsActive);
    }

    private static EmployeeContractData Data(
        string? contractNumber = "V-100",
        Guid? employeeId = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        bool isPermanent = true,
        string? employmentType = "Vollzeit",
        decimal weeklyHours = 40,
        decimal monthlyHours = 173,
        decimal vacationDays = 30,
        DateOnly? probationEndDate = null) =>
        new(
            contractNumber,
            employeeId ?? Guid.NewGuid(),
            startDate ?? new DateOnly(2026, 2, 1),
            endDate,
            isPermanent,
            employmentType,
            weeklyHours,
            monthlyHours,
            vacationDays,
            probationEndDate,
            null);
}
