using Aviator.Global.Config;
using Aviator.Global.Config.Implementation;
using InfluxDB3.Client;
using InfluxDB3.Client.Config;
using InfluxDB3.Client.Write;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aviator.Global.Extensions.TimeSeries;

public static class InfluxDbTimeSeriesExtension
{
    public static void AddAviatorInfluxDb(this WebApplicationBuilder builder)
    {
        var influxDbConfig = builder.Configuration
            .GetSection(MetricsConfig.Section)
            .GetSection(InfluxDbConfig.Section).Get<InfluxDbConfig>();

        ArgumentNullException.ThrowIfNull(influxDbConfig);

        if (!influxDbConfig.Enabled)
        {
            return;
        }

        var clientConfig = new ClientConfig
        {
            Host = influxDbConfig.Host,
            Organization = influxDbConfig.Organization,
            Database = influxDbConfig.Bucket,
            Token = influxDbConfig.Token,
            Timeout = TimeSpan.FromSeconds(60),
            WriteOptions = new WriteOptions
            {
                Precision = WritePrecision.S
            }
        };

        builder.Services.AddSingleton(new InfluxDBClient(clientConfig));
    }
}