using System.Net;
using Aviator.Adsb.Entities;
using Aviator.Global.Metrics;
using InfluxDB3.Client;
using InfluxDB3.Client.Write;
using Microsoft.Extensions.Logging;

namespace Aviator.Adsb.Metrics;

public class InfluxDbAdsbMetrics(InfluxDbMetrics client, ILogger<InfluxDbAdsbMetrics> logger) : IAdsbMetrics
{
    public async Task IncreaseAsync(AdsbStats adsbStats, CancellationToken cancellationToken = default)
    {
        try
        {   
            var point = PointData.Measurement("adsb")
                .SetField("aircraftTotal", adsbStats.AircraftTotal)
                .SetField("messagesValid", adsbStats.MessagesValid)
                .SetField("messagesInvalid", adsbStats.MessagesInvalid)
                .SetField("gain", adsbStats.Gain);

            await client.WritePointAsync(point, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to write metric.");
        }
    }
}