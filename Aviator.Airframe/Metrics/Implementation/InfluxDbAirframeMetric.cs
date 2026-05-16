using System.Collections.Concurrent;
using System.Net;
using InfluxDB3.Client;
using InfluxDB3.Client.Write;
using Microsoft.Extensions.Logging;

namespace Aviator.Airframe.Metrics.Implementation;

public class InfluxDbAirframeMetric(ILogger<InfluxDbAirframeMetric> logger, InfluxDBClient influxDbClient) : IAirframeMetric
{
    private bool _disabled;

    private async Task WritePointAsync(PointData pointData, CancellationToken cancellationToken = default)
    {
        if (_disabled)
        {
            return;
        }

        try
        {
            await influxDbClient
                .WritePointAsync(pointData, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (InfluxDBApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            logger.LogWarning(ex, "Not authorized! We disable the Metrics for now, please adjust the config and try again.");
            _disabled = true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to write DataPoint to InfluxDB... {LineProtocol}", pointData.ToLineProtocol());
        }
    }

    public async Task WriteAirframeAsync(Frames.Entities.Airframe airframe, CancellationToken cancellationToken = default)
    {
        var point = PointData.Measurement("airframes")
            .SetTag("channel", airframe.Channel)
            .SetTag("frameType", airframe.FrameType.ToString())
            .SetTag("protocolType", airframe.ProtocolType.ToString())
            .SetField("value", 1);

        await WritePointAsync(point, cancellationToken).ConfigureAwait(false);
        
        if (airframe.SignalLevel is not null)
        {
            var signalLevel = PointData.Measurement("signalLevel")
                .SetTag("channel", airframe.Channel)
                .SetTag("frameType", airframe.FrameType.ToString())
                .SetField("value", airframe.SignalLevel);
            
            await WritePointAsync(signalLevel, cancellationToken).ConfigureAwait(false);
        }

        if (airframe.NoiseLevel is not null)
        {
            var signalNoise = PointData.Measurement("noiseLevel")
                .SetTag("channel", airframe.Channel)
                .SetTag("frameType", airframe.FrameType.ToString())
                .SetField("value", airframe.NoiseLevel);
            
            await WritePointAsync(signalNoise, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task WriteCounterAsync(ConcurrentDictionary<AirframeCounterKey, int> aggregatedCount, DateTime timestamp, CancellationToken cancellationToken = default)
    {
        foreach (var kvp in aggregatedCount)
        {
            var point = PointData.Measurement("airframesCounter")
                .SetTag("channel", kvp.Key.Channel)
                .SetTag("frameType", kvp.Key.FrameType.ToString())
                .SetField("value", (long)kvp.Value)
                .SetTimestamp(timestamp);

            await WritePointAsync(point, cancellationToken).ConfigureAwait(false);
        }
    }
}