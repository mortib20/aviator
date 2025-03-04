using System.Net;
using Aviator.Acars.Config;
using Aviator.Acars.Entities;
using InfluxDB3.Client;
using InfluxDB3.Client.Config;
using InfluxDB3.Client.Write;
using Microsoft.Extensions.Logging;

namespace Aviator.Acars.Metrics;

public class InfluxDbMetrics(InfluxDbConfig influxConfig, ILogger<InfluxDbMetrics> logger): IAcarsMetrics
{
    private readonly InfluxDBClient _client = new(new ClientConfig
    {
        Host = influxConfig.Url,
        Organization = influxConfig.Organization,
        Database = influxConfig.Bucket,
        Token = influxConfig.Token,
        WriteOptions = new WriteOptions
        {
            Precision = WritePrecision.S
        },
        Timeout = TimeSpan.FromSeconds(60)
    });

    private bool _disabled;

    public async Task IncreaseAsync(AirFrame frame, CancellationToken cancellationToken = default)
    {
        if (_disabled)
        {
            return;
        }
        
        try
        {
            var point = PointData.Measurement("frames")
                .SetTag("sourceType", frame.SourceType.ToString())
                .SetTag("frameType", frame.FrameType.ToString())
                .SetTag("channel", frame.Channel)
                .SetField("sigLevel", frame.SigLevel)
                .SetField("noiseLevel", frame.NoiseLevel)
                .SetField("value", 1);

            await _client.WritePointAsync(point, cancellationToken: cancellationToken);
        }
        catch (InfluxDBApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            logger.LogWarning(ex, "Not authorized! We disable the Metrics for now, please adjust the config and restart the service. {@Frame}", frame);
            _disabled = true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to write metric. {@Frame}", frame);
        }
    }
}