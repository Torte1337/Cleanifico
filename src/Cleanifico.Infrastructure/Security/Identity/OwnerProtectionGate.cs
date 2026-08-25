namespace Cleanifico.Infrastructure.Security.Identity;

public sealed class OwnerProtectionGate
{
    internal SemaphoreSlim Semaphore { get; } = new(1, 1);
}
