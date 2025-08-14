using InfluxDB3.Client;
using InfluxDB3.Client.Write;
using Microsoft.Extensions.Logging;

namespace Aviator.Adsb.Metrics.Implementation;

public class InfluxDbAdsbMetrics(ILogger<InfluxDbAdsbMetrics> logger, InfluxDBClient client) : IAdsbMetrics
{
    public async Task IncreaseAsync(Dictionary<string, decimal> adsbStats, CancellationToken cancellationToken = default)
    {
        try
        {
            var point = PointData.Measurement("adsb");
            
            foreach (var keyValuePair in adsbStats)
            {   
                point.SetField(keyValuePair.Key, keyValuePair.Value);
            }

            await client.WritePointAsync(point, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to write metric.");
        }
    }
}