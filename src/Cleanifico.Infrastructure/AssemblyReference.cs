using System.Reflection;

namespace Cleanifico.Infrastructure;

/// <summary>
/// Provides a stable reference to the infrastructure assembly for registration and architecture tests.
/// </summary>
public static class AssemblyReference
{
    public static Assembly Value { get; } = typeof(AssemblyReference).Assembly;
}
