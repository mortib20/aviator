using InfluxDB3.Client.Write;

namespace Aviator.Global.Metrics;

public interface IMetrics
{
    Task WritePointAsync(PointData pointData, CancellationToken cancellationToken = default);
}