using System.Reflection;

namespace Cleanifico.Application;

/// <summary>
/// Provides a stable reference to the application assembly for registration and architecture tests.
/// </summary>
public static class AssemblyReference
{
    public static Assembly Value { get; } = typeof(AssemblyReference).Assembly;
}
