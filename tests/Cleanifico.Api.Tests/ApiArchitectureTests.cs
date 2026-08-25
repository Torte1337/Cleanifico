using Cleanifico.Tests.Architecture;

namespace Cleanifico.Api.Tests;

public sealed class ApiArchitectureTests
{
    [Fact]
    public void ApiProject_ReferencesApplicationContractsAndInfrastructure()
    {
        string[] expected =
        [
            "Cleanifico.Application",
            "Cleanifico.Contracts",
            "Cleanifico.Infrastructure"
        ];

        var actual = RepositoryStructure.ReadProjectReferences(
            "src/Cleanifico.Api/Cleanifico.Api.csproj");

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void WebProject_ReferencesOnlyContracts()
    {
        string[] expected = ["Cleanifico.Contracts"];

        var actual = RepositoryStructure.ReadProjectReferences(
            "src/Cleanifico.Web/Cleanifico.Web.csproj");

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Solution_ContainsAllInitialProjects()
    {
        string[] expected =
        [
            "Cleanifico.Api",
            "Cleanifico.Api.Tests",
            "Cleanifico.Application",
            "Cleanifico.Application.Tests",
            "Cleanifico.Contracts",
            "Cleanifico.Domain",
            "Cleanifico.Domain.Tests",
            "Cleanifico.Infrastructure",
            "Cleanifico.Infrastructure.Tests",
            "Cleanifico.Web",
            "Cleanifico.Web.Tests"
        ];

        var actual = RepositoryStructure.ReadSolutionProjects();

        Assert.Equal(expected, actual);
    }
}
