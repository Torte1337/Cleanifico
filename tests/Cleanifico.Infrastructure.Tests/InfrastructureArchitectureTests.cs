using Cleanifico.Tests.Architecture;

namespace Cleanifico.Infrastructure.Tests;

public sealed class InfrastructureArchitectureTests
{
    [Fact]
    public void InfrastructureProject_ReferencesOnlyApplicationAndDomain()
    {
        string[] expected = ["Cleanifico.Application", "Cleanifico.Domain"];

        var actual = RepositoryStructure.ReadProjectReferences(
            "src/Cleanifico.Infrastructure/Cleanifico.Infrastructure.csproj");

        Assert.Equal(expected, actual);
    }
}
