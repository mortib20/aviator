using Aviator.Global.Metrics.Implementation;
using InfluxDB3.Client.Write;
using QuestDB;
using QuestDB.Utils;

namespace Aviator.Airframe.Metrics;

public class AirframeMetrics(InfluxDbMetrics influxDbMetrics, SenderOptions senderOptions)
{
    public async Task HandleAirframe(Frames.Entities.Airframe airframe)
    {
        using var sender = Sender.New(senderOptions);

        await sender.Table("airframes")
            .Symbol("Channel", airframe.Channel)
            .Symbol("FrameType", airframe.FrameType.ToString())
            .Symbol("ProtocolType", airframe.ProtocolType.ToString())
            .Column("value", 1)
            .AtAsync(DateTime.UtcNow);

        await sender.SendAsync();
        
        var point = PointData.Measurement("airframes")
            .SetTag("channel", airframe.Channel)
            .SetTag("frameType", airframe.FrameType.ToString())
            .SetTag("protocolType", airframe.ProtocolType.ToString())
            .SetField("value", 1);

        await influxDbMetrics.WritePointAsync(point).ConfigureAwait(false);
        
        if (airframe.SignalLevel is not null)
        {
            var signalLevel = PointData.Measurement("signalLevel")
                .SetTag("channel", airframe.Channel)
                .SetTag("frameType", airframe.FrameType.ToString())
                .SetField("value", airframe.SignalLevel);
            
            await influxDbMetrics.WritePointAsync(signalLevel).ConfigureAwait(false);
        }

        if (airframe.NoiseLevel is not null)
        {
            var signalNoise = PointData.Measurement("noiseLevel")
                .SetTag("channel", airframe.Channel)
                .SetTag("frameType", airframe.FrameType.ToString())
                .SetField("value", airframe.NoiseLevel);
            
            await influxDbMetrics.WritePointAsync(signalNoise).ConfigureAwait(false);
        }
    }
}