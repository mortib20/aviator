using Aviator.Adsb.Config;
using Aviator.Adsb.Entities;
using Aviator.Adsb.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Aviator.Adsb;

public class PrometheusMetricsConverterService(ILogger<PrometheusMetricsConverterService> logger, IAdsbMetrics metrics, AdsbConfig config) : BackgroundService 
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var statsPath = Path.GetDirectoryName(config.StatsPath);
        var statsFile = Path.GetFileName(config.StatsPath);

        if (!Path.Exists(statsPath))
        {
            logger.LogWarning("Directory {Directory} does not exist...", statsPath);
            return;
        }

        if (!File.Exists(config.StatsPath))
        {
            logger.LogWarning("File {File} does not exist...", statsFile);
            return;
        }

        logger.LogInformation("Starting {Type} and watching {Path} {File}", this, statsPath, statsFile);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("File changed going to convert and put metrics out! {Path} {File}", statsPath, statsFile);
            var fileContent = await File.ReadAllLinesAsync(config.StatsPath, stoppingToken).ConfigureAwait(false);

            var stats = new AdsbStats
            {
                AircraftTotal = int.Parse(fileContent.First(s => s.Contains("readsb_aircraft_total")).Split(' ')[1]),
                Gain = int.Parse(fileContent.First(s => s.Contains("readsb_sdr_gain")).Split(' ')[1]),
                MessagesValid = int.Parse(fileContent.First(s => s.Contains("readsb_messages_valid")).Split(' ')[1]),
                MessagesInvalid = int.Parse(fileContent.First(s => s.Contains("readsb_messages_invalid")).Split(' ')[1]),
            };
            
            await metrics.IncreaseAsync(stats, stoppingToken).ConfigureAwait(false);
            
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken).ConfigureAwait(false);
        }
    }
}