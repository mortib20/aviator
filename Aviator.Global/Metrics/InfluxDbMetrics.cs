using Aviator.Main.Config;
using InfluxDB3.Client;
using InfluxDB3.Client.Config;
using InfluxDB3.Client.Write;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aviator.Global.Metrics;

public class InfluxDbMetrics(ILogger<InfluxDbMetrics> logger, IOptions<InfluxDbConfig> influxDbConfig)
{
    private readonly InfluxDBClient _influxDbClient = SetupClient(influxDbConfig.Value);

    public async Task WritePointAsync(PointData pointData, CancellationToken cancellationToken = default)
    {
        try
        {
            await _influxDbClient.WritePointAsync(point: pointData, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to write DataPoint to InfluxDB... {LineProtocol}", pointData.ToLineProtocol());
        }
    }
    
    private static InfluxDBClient SetupClient(InfluxDbConfig config)
    {
        return new InfluxDBClient(new ClientConfig()
        {
            Host = config.Url,
            Organization = config.Organization,
            Database = config.Bucket,
            Token = config.Token,
            WriteOptions = new WriteOptions
            {
                Precision = WritePrecision.S
            },
            Timeout = TimeSpan.FromSeconds(60)
        });
    } 
}