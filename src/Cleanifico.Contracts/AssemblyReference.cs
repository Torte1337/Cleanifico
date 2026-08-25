using System.Reflection;

namespace Cleanifico.Contracts;

/// <summary>
/// Provides a stable reference to the public contracts assembly.
/// </summary>
public static class AssemblyReference
{
    public static Assembly Value { get; } = typeof(AssemblyReference).Assembly;
}
