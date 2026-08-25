using Cleanifico.Tests.Architecture;

namespace Cleanifico.Domain.Tests;

public sealed class DomainArchitectureTests
{
    [Fact]
    public void DomainProject_HasNoProjectDependencies()
    {
        var references = RepositoryStructure.ReadProjectReferences(
            "src/Cleanifico.Domain/Cleanifico.Domain.csproj");

        Assert.Empty(references);
    }

    [Fact]
    public void DomainAssembly_DoesNotReferenceOtherCleanificoAssemblies()
    {
        var cleanificoReferences = AssemblyReference.Value
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .Where(name => name?.StartsWith("Cleanifico.", StringComparison.Ordinal) is true);

        Assert.Empty(cleanificoReferences);
    }
}
