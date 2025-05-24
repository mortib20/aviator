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
        
        // TODO noise and signal level

        await influxDbMetrics.WritePointAsync(point).ConfigureAwait(false);
    }
}