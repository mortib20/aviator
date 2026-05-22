using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Aviator.Airframe.Frames.Strategies.Hfdl.DumpHfdl.Protocol;

public class DumpHfdlAcarsStrategy(ILogger<DumpHfdlAcarsStrategy> logger) : IDumpHfdlProtocolStrategy
{
    public ProtocolType ProtocolType => ProtocolType.Acars;

    public bool CanHandleProtocol(JsonElement protocol)
    {
        return protocol.TryGetProperty("hfnpdu", out var hfnpdu)
               && hfnpdu.TryGetProperty("acars", out var acars)
               && acars.ValueKind == JsonValueKind.Object;
    }

    public Task<object> HandleProtocolAsync(JsonElement protocol, CancellationToken cancellationToken)
    {
        using var scope = logger.BeginScope(nameof(DumpHfdlAcarsStrategy));
        logger.LogDebug("Handling Acars here");


        var hasHfnpdu = protocol.TryGetProperty("hfnpdu", out var hfnpdu);
        var hasAcars = hfnpdu.TryGetProperty("acars", out var acars);

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