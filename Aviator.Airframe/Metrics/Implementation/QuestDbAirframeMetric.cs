using Aviator.Airframe.Frames.Entities;
using Aviator.Global.TimeSeries;
using Microsoft.Extensions.Logging;

namespace Aviator.Airframe.Metrics.Implementation;

public class QuestDbAirframeMetric(ILogger<QuestDbAirframeMetric> logger, QuestDbClient questDbClient) : IAirframeMetric
{
    public async Task WriteAirframeAsync(Frames.Entities.Airframe airframe, CancellationToken cancellationToken = default)
    {
        using var sender = questDbClient.GetSender();

        await sender.Table("airframes")
            .Symbol("channel", airframe.Channel)
            .Symbol("frameType", airframe.FrameType.ToString())
            .Symbol("protocolType", airframe.ProtocolType.ToString())
            .Column("value", 1)
            .AtAsync(DateTime.UtcNow, cancellationToken)
            .ConfigureAwait(false);

        // Handle Acars Protocol
        if (airframe.Position is not null)
        {
            await sender.Table("airframePositions")
                .Symbol("icao", string.IsNullOrEmpty(airframe.Icao) ? null : airframe.Icao)
                .Symbol("frameType", airframe.FrameType.ToString())
                .Column("latitude", airframe.Position.Latitude)
                .Column("longitude", airframe.Position.Longitude)
                .AtAsync(DateTime.UtcNow, cancellationToken)
                .ConfigureAwait(false);
        }

        // noise and signal level

        await sender.SendAsync(cancellationToken).ConfigureAwait(false);
    }
}