using Aviator.Global.Metrics.Implementation;
using InfluxDB3.Client.Write;

namespace Aviator.Airframe.Metrics;

public class AirframeMetrics(InfluxDbMetrics influxDbMetrics)
{
    public async Task HandleAirframe(Frames.Entities.Airframe airframe)
    {
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