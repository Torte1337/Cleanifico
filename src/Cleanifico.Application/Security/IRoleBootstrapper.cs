namespace Cleanifico.Application.Security;

public interface IRoleBootstrapper
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
