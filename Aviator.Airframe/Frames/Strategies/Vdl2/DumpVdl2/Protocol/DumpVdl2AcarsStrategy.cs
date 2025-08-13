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
            Label = label.GetString() ?? string.Empty,
            Registration = reg.GetString() ?? string.Empty,
            Text = msgText.GetString() ?? string.Empty
        };
        
        return Task.FromResult<object>(protocolResult);
    }
}