using System.Reflection;

namespace Cleanifico.Domain;

/// <summary>
/// Provides a stable reference to the domain assembly for architecture tests and assembly scanning.
/// </summary>
public static class AssemblyReference
{
    public static Assembly Value { get; } = typeof(AssemblyReference).Assembly;
}
