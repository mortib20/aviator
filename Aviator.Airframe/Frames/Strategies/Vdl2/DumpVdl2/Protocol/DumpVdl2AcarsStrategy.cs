using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Aviator.Airframe.Frames.Strategies.Vdl2.DumpVdl2.Protocol;

public class DumpVdl2AcarsStrategy(ILogger<DumpVdl2AcarsStrategy> logger) : IDumpVdl2ProtocolStrategy
{
    public ProtocolType ProtocolType => ProtocolType.Acars;
    public bool CanHandleProtocol(JsonElement protocol)
    {
        return
            protocol.TryGetProperty("acars", out var acars)
            && acars.ValueKind == JsonValueKind.Object;
    }

    public Task<object> HandleProtocolAsync(JsonElement protocol, CancellationToken cancellationToken)
    {
        using var scope = logger.BeginScope(nameof(DumpVdl2AcarsStrategy));
        logger.LogDebug("Handling Acars here");
        
        var hasAcars = protocol.TryGetProperty("acars", out var acars);

        var hasLabel = acars.TryGetProperty("label", out var label);
        var hasReg = acars.TryGetProperty("reg", out var reg);
        var hasMsgText = acars.TryGetProperty("msg_text", out var msgText);

        if (!hasLabel && !hasReg && !hasMsgText)
        {
            return Task.FromResult(new object());
        }

        var protocolResult = new Entities.Acars
        {
            Label = hasLabel ? label.GetString() ?? string.Empty : string.Empty,
            Registration = hasReg ? reg.GetString() ?? string.Empty : string.Empty,
            FlightNumber = acars.TryGetProperty("flight", out var flt) ? flt.GetString() ?? string.Empty : string.Empty,
            MessageNumber = acars.TryGetProperty("msg_num", out var msgNum) ? msgNum.GetString() ?? string.Empty : string.Empty,
            Text = hasMsgText ? msgText.GetString() ?? string.Empty : string.Empty
        };
        
        return Task.FromResult<object>(protocolResult);
    }
}