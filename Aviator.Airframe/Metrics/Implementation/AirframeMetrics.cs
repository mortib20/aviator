using Microsoft.Extensions.Logging;

namespace Aviator.Airframe.Metrics.Implementation;

public class AirframeMetrics(ILogger<AirframeMetrics> logger, List<IAirframeMetric> airframeMetrics) : IAirframeMetric
{
    public async Task WriteAirframeAsync(Frames.Entities.Airframe airframe, CancellationToken cancellationToken = default)
    {
        foreach (var airframeMetric in airframeMetrics)
        {
            using var scope = logger.BeginScope(airframeMetric.GetType().Name);
            try
            {
                await airframeMetric.WriteAirframeAsync(airframe, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send metric.");
            }
        }
    }
}