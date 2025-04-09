using System.Globalization;
using Aviator.Adsb.Config;
using Aviator.Adsb.Entities;
using Aviator.Adsb.Metrics;
using Aviator.Global.Service;
using Microsoft.Extensions.Logging;

namespace Aviator.Adsb;

// TODO Better Error Handling and Rename?
public class PrometheusMetricsConverterService(ILogger<PrometheusMetricsConverterService> logger, IAdsbMetrics metrics, AdsbConfig config) : AviatorBackgroundService(logger)
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var statsPath = Path.GetDirectoryName(config.StatsPath);
        var statsFile = Path.GetFileName(config.StatsPath);

        logger.LogInformation("Watching {Path} {File}", statsPath, statsFile);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (StatsExist(statsPath, statsFile))
                {
                    var fileLines = await File.ReadAllLinesAsync(config.StatsPath, stoppingToken).ConfigureAwait(false);

                    // Split stats<SPACE>value
                    var metricsDict = fileLines.Select(fileLine => fileLine.Split(' ')).ToDictionary(metric => metric[0], metric => decimal.Parse(metric[1]));

                    await metrics.IncreaseAsync(metricsDict, stoppingToken).ConfigureAwait(false);
                }

                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken).ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get ADS-B Metrics!");
        }
    }

    private bool StatsExist(string? statsPath, string statsFile)
    {
        if (!Path.Exists(statsPath))
        {
            logger.LogWarning("Directory {Directory} does not exist...", statsPath);
            return false;
        }

        if (!File.Exists(config.StatsPath))
        {
            logger.LogWarning("File {File} does not exist...", statsFile);
            return false;
        }

        return true;
    }
}
