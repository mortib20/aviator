using Aviator.Global.Config;
using Aviator.Global.Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Aviator.Global.DependencyInjection;

public static class InfluxDbExtension
{
    public static WebApplicationBuilder AddAviatorInfluxDb(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<InfluxDbConfig>(builder.Configuration.GetSection(MetricsConfig.Section)
            .GetSection(InfluxDbConfig.Section));

        builder.Services.AddSingleton<InfluxDbMetrics>();

        return builder;
    }
}