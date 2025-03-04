using Aviator.Acars.Entities;

namespace Aviator.Acars.Metrics;

public interface IAcarsMetrics
{
    Task IncreaseAsync(AirFrame frame, CancellationToken cancellationToken = default);
}