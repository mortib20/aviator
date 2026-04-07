using Aviator.Global.Config;
using Aviator.Global.Config.Implementation;
using Aviator.Global.TimeSeries;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aviator.Global.Extensions.TimeSeries;

public static class QuestDbTimeSeriesExtension
{
    public static void AddQuestDb(this WebApplicationBuilder builder)
    {
        var questDbConfig = builder.Configuration
            .GetSection(MetricsConfig.Section)
            .GetSection(QuestDbConfig.Section)
            .Get<QuestDbConfig>();

        ArgumentNullException.ThrowIfNull(questDbConfig);

        if (!questDbConfig.Enabled)
        {
            return;
        }

        // Flush every 1000 rows, or each minute
        var connectionString = $"http::addr={questDbConfig.Host};auto_flush=on;auto_flush_rows=1000;auto_flush_interval=61000;";
        
        builder.Services.AddSingleton(new QuestDbClient(connectionString));
    }
}