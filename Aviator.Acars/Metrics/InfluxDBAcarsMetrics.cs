using Aviator.Acars.Entities;
using Aviator.Global.Metrics;
using InfluxDB3.Client.Write;
using Microsoft.Extensions.Logging;

namespace Aviator.Acars.Metrics;

public class InfluxDBAcarsMetrics(InfluxDbMetrics client, ILogger<InfluxDBAcarsMetrics> logger): IAcarsMetrics
{
    public async Task IncreaseAsync(AirFrame frame, CancellationToken cancellationToken = default)
    {
        try
        {
            var point = PointData.Measurement("frames")
                .SetTag("sourceType", frame.SourceType.ToString())
                .SetTag("frameType", frame.FrameType.ToString())
                .SetTag("channel", frame.Channel)
                .SetField("sigLevel", frame.SigLevel)
                .SetField("noiseLevel", frame.NoiseLevel)
                .SetField("value", 1);

            await client.WritePointAsync(point, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to write metric. {@Frame}", frame);
        }
    }
}