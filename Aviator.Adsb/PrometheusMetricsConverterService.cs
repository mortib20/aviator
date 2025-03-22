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

        logger.LogInformation("Starting {Type} and watching {Path} {File}", this, statsPath, statsFile);
        
        var watcher = new FileSystemWatcher(statsPath, statsFile);
        watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
        watcher.EnableRaisingEvents = true;
        watcher.Changed += async (sender, e) => await WatcherOnChanged(sender, e, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task WatcherOnChanged(object sender, FileSystemEventArgs e, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("File changed going to convert and put metrics out! {Path} {File}", e.FullPath, e.Name);
        var fileContent = await File.ReadAllLinesAsync(e.FullPath, cancellationToken).ConfigureAwait(false);

        var stats = new AdsbStats()
        {
            AircraftTotal = int.Parse(fileContent.First(s => s.Contains("readsb_aircraft_total")).Split(' ')[1])
        };

        await metrics.IncreaseAsync(stats, cancellationToken).ConfigureAwait(false);
    }
}