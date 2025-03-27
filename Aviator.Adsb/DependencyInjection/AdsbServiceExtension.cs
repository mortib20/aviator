using System.Collections.ObjectModel;
using Aviator.Adsb.Config;
using Aviator.Adsb.Metrics;
using Aviator.Global.Config;
using Aviator.Global.Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Aviator.Adsb.DependencyInjection;

public static class AdsbServiceExtension
{
    public static WebApplicationBuilder AddAdsbService(this WebApplicationBuilder builder)
    {
        var adsbConfig = builder.Configuration.GetSection(AdsbConfig.Section).Get<AdsbConfig>();
        ArgumentNullException.ThrowIfNull(adsbConfig);

        var metricsConfig = builder.Configuration.GetSection(MetricsConfig.Section).Get<MetricsConfig>();
        ArgumentNullException.ThrowIfNull(metricsConfig);

        SetupMetrics(builder, metricsConfig);

        builder.Services.AddHostedService<PrometheusMetricsConverterService>(s => new PrometheusMetricsConverterService(s.GetRequiredService<ILogger<PrometheusMetricsConverterService>>(), s.GetRequiredService<IAdsbMetrics>(), adsbConfig));

        return builder;
    }

    private static void SetupMetrics(WebApplicationBuilder builder, MetricsConfig metricsConfig)
    {
        builder.Services.AddSingleton<IAdsbMetrics>(s =>
        {
            var logger = s.GetRequiredService<ILogger<InfluxDbAdsbMetrics>>();
            var metrics = new Collection<IAdsbMetrics>();

            if (metricsConfig.InfluxDb is not null && metricsConfig.InfluxDb!.Enabled)
            {
                var metricLogger = s.GetRequiredService<ILogger<InfluxDbAdsbMetrics>>();
                var metric = new InfluxDbAdsbMetrics(s.GetRequiredService<InfluxDbMetrics>(), metricLogger);
                metrics.Add(metric);
            }

            var enabledMetrics = metrics.Select(acarsMetrics => acarsMetrics.GetType()).ToList();
            logger.LogInformation("Enabled Metric: {Types}", string.Join(", ", enabledMetrics));

            return new AdsbMetrics(metrics);
        });
    }
}