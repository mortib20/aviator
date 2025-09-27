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
            Label = label.GetString() ?? string.Empty,
            Registration = reg.GetString() ?? string.Empty,
            Text = msgText.GetString() ?? string.Empty
        };

        return Task.FromResult<object>(protocolResult);
    }
}