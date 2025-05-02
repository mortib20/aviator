using Aviator.Acars.Entities;

namespace Aviator.Acars.Metrics;

public interface IAirframeMetrics
{
    Task IncreaseAsync(AirFrame frame, CancellationToken cancellationToken = default);
}