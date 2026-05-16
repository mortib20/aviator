using System.Collections.Concurrent;
using System.Timers;
using Aviator.Airframe.Frames;
using Aviator.Global.Extensions.Methods;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace Aviator.Airframe.Metrics.Implementation;

public record AirframeCounterKey(string Channel, FrameType FrameType);

public class AirframeMetrics : IDisposable
{
    private readonly ILogger<AirframeMetrics> _logger;
    private readonly List<IAirframeMetric> _airframeMetrics;
    
    private readonly Timer _sendMetricTimer = new(TimeSpan.FromMinutes(1));
    
    private ConcurrentDictionary<AirframeCounterKey, int> _airframePerTimeSpanCounter = new();

    public AirframeMetrics(ILogger<AirframeMetrics> logger, List<IAirframeMetric> airframeMetrics)
    {
        _logger = logger;
        _airframeMetrics = airframeMetrics;
        
        // Add EventHandler
        _sendMetricTimer.Elapsed += SendMetricTimerOnElapsed; 
        
        // Start Metric Timer
        _sendMetricTimer.Start();
    }

    private void SendMetricTimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        using var metricTimerOnElapsedScope = _logger.BeginScope("SendMetricTimerOnElapsed");
        _logger.LogDebug("Timer Elapsed at {time:O}", e.SignalTime);
        
        // Make a snapshot of the current minute and replace Dictionary with empty dictionary
        var snapshot = Interlocked.Exchange(ref _airframePerTimeSpanCounter, new ConcurrentDictionary<AirframeCounterKey, int>());

        // Nothing received in the timespan, return
        if (snapshot.IsEmpty)
        {
            return;
        }
            
        // Get the current minute
        var now = e.SignalTime.ToUniversalTime().AddSeconds(-30);
        var alignedTimestamp = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, DateTimeKind.Utc);
        
        SendMetric(snapshot, alignedTimestamp)
            .FireAndForget(_logger, $"Sending {snapshot.Count} metrics for {alignedTimestamp:T}");
    }

    private async Task SendMetric(ConcurrentDictionary<AirframeCounterKey, int> aggregated, DateTime timestamp, CancellationToken cancellationToken = default)
    {
        foreach (var airframeMetric in _airframeMetrics)
        {
            using var scope = _logger.BeginScope(airframeMetric.GetType().Name);
            try
            {
                await airframeMetric.WriteCounterAsync(aggregated, timestamp, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send metric.");
            }
        }
    }

    public void Dispose()
    {
        _sendMetricTimer.Stop();
        _sendMetricTimer.Dispose();
    }

    public async Task HandleAirframeAsync(Frames.Entities.Airframe airframe, CancellationToken cancellationToken = default)
    {
        using var handleAirframeCope = _logger.BeginScope("HandleAirframeAsync");
        _logger.LogDebug("Received Airframe...");
        
        // here we get the frame, frame has 3 metrics,
        // - an increasing counter with (Channel, FrameType),
        // - signal strength, noise strength
        // - if not null a position
        // we sample every minute and send to time series dbs
        
        // counter
        var counterKey = new AirframeCounterKey(airframe.Channel, airframe.FrameType);
        _airframePerTimeSpanCounter.AddOrUpdate(counterKey, 1, (_, count) => count + 1);

        // other
        foreach (var metric in _airframeMetrics)
        {
            await metric.WriteAirframeAsync(airframe, cancellationToken).ConfigureAwait(false);
        }
    }
}