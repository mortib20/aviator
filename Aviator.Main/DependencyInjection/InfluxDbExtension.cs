using Aviator.Acars.Config;
using Aviator.Main.Config;
using InfluxDB3.Client;
using InfluxDB3.Client.Config;
using InfluxDB3.Client.Write;

namespace Aviator.Main.DependencyInjection;

public static class InfluxDbExtension
{
    public static WebApplicationBuilder AddAviatorInfluxDb(this WebApplicationBuilder builder)
    {
        var influxConfig = builder.Configuration.GetSection(MetricsConfig.Section).GetSection("InfluxDB").Get<InfluxDbConfig>();
        ArgumentNullException.ThrowIfNull(influxConfig);
        
        var client = new InfluxDBClient(new ClientConfig
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
        
        builder.Services.AddSingleton(client);
        
        return builder;
    }
}