namespace Cleanifico.Application.TimeTypes;

public interface ITimeTypeInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
