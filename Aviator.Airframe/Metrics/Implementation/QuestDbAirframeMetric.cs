using System.Collections.Concurrent;
using Aviator.Global.TimeSeries;
using Microsoft.Extensions.Logging;
using QuestDB.Senders;

namespace Aviator.Airframe.Metrics.Implementation;

public class QuestDbAirframeMetric(ILogger<QuestDbAirframeMetric> logger, QuestDbClient questDbClient) : IAirframeMetric
{
    private readonly ISender _sender = questDbClient.GetSender();
    
    public async Task WriteAirframeAsync(Frames.Entities.Airframe airframe, CancellationToken cancellationToken = default)
    {

        // Handle Acars Protocol
        if (airframe.Position is not null)
        {
            await _sender.Table("airframePositions")
                .Symbol("frameType", airframe.FrameType.ToString())
                .Column("icao", string.IsNullOrEmpty(airframe.Icao) ? airframe.Source.Address : airframe.Icao)
                .Column("latitude", (double)airframe.Position.Latitude)
                .Column("longitude", (double)airframe.Position.Longitude)
                .AtAsync(DateTime.UtcNow, cancellationToken)
                .ConfigureAwait(false);
        }

        // noise and signal level
        if (airframe.SignalLevel is not null)
        {
            var signalLevel = (double)airframe.SignalLevel;
            
            await _sender.Table("airframesSignal")
                .Symbol("channel", airframe.Channel)
                .Symbol("frameType", airframe.FrameType.ToString())
                .Column("value", signalLevel)
                .AtAsync(DateTime.UtcNow, cancellationToken)
                .ConfigureAwait(false);
        }
        
        logger.LogDebug("Send stuff to QuestDB");

        // await sender.SendAsync(cancellationToken).ConfigureAwait(false);
    }
    
    public async Task WriteCounterAsync(ConcurrentDictionary<AirframeCounterKey, int> aggregatedCount, DateTime timestamp, CancellationToken cancellationToken = default)
    {
        using var sender = questDbClient.GetSender();

        foreach (var kvp in aggregatedCount)
        {
            await sender.Table("airframes")
                .Symbol("channel", kvp.Key.Channel)
                .Symbol("frameType", kvp.Key.FrameType.ToString())
                .Column("value", kvp.Value)
                .AtAsync(timestamp, cancellationToken)
                .ConfigureAwait(false);
        }
        
        await sender.SendAsync(cancellationToken).ConfigureAwait(false);
    }
}