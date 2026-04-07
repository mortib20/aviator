using System.Collections.Concurrent;
using Aviator.Airframe.Metrics.Implementation;

namespace Aviator.Airframe.Metrics;

public interface IAirframeMetric
{
    Task WriteAirframeAsync(Frames.Entities.Airframe airframe, CancellationToken cancellationToken = default);
    Task WriteCounterAsync(ConcurrentDictionary<AirframeCounterKey, int> aggregatedCount, DateTime timestamp, CancellationToken cancellationToken = default);
}