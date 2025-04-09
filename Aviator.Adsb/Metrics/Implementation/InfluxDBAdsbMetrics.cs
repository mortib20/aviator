using Aviator.Adsb.Entities;
using Aviator.Global.Metrics.Implementation;
using InfluxDB3.Client.Write;
using Microsoft.Extensions.Logging;

namespace Aviator.Adsb.Metrics.Implementation;

public class InfluxDbAdsbMetrics(InfluxDbMetrics client, ILogger<InfluxDbAdsbMetrics> logger) : IAdsbMetrics
{
    public async Task IncreaseAsync(Dictionary<string, string> adsbStats, CancellationToken cancellationToken = default)
    {
        try
        {


            var point = PointData.Measurement("adsb");
            
            foreach (var keyValuePair in adsbStats)
            {
                point.SetField(keyValuePair.Key, keyValuePair.Value);
            }
            
            logger.LogDebug("{LineProtocol}", point.ToLineProtocol());

            await client.WritePointAsync(point, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to write metric.");
        }
    }
}