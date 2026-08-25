using Cleanifico.Tests.Architecture;

namespace Cleanifico.Application.Tests;

public sealed class ApplicationArchitectureTests
{
    [Fact]
    public void ApplicationProject_ReferencesOnlyDomainAndContracts()
    {
        string[] expected = ["Cleanifico.Contracts", "Cleanifico.Domain"];

        var actual = RepositoryStructure.ReadProjectReferences(
            "src/Cleanifico.Application/Cleanifico.Application.csproj");

        Assert.Equal(expected, actual);
    }
}
