using System.Net;
using Aviator.Adsb.Entities;
using InfluxDB3.Client;
using InfluxDB3.Client.Write;
using Microsoft.Extensions.Logging;

namespace Aviator.Adsb.Metrics;

public class InfluxDbAdsbMetrics(InfluxDBClient client, ILogger<InfluxDbAdsbMetrics> logger) : IAdsbMetrics
{
    private bool _disabled;
    
    public async Task IncreaseAsync(AdsbStats adsbStats, CancellationToken cancellationToken = default)
    {
        if (_disabled)
        {
            return;
        }
        
        try
        {
            var point = PointData.Measurement("adsb")
                .SetField("aircraftTotal", adsbStats.AircraftTotal);

            await client.WritePointAsync(point, cancellationToken: cancellationToken);
        }
        catch (InfluxDBApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            logger.LogWarning(ex, "Not authorized! We disable the Metrics for now, please adjust the config and restart the service.");
            _disabled = true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to write metric.");
        }
    }
}