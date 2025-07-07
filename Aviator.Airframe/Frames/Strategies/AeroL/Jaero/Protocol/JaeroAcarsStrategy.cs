using System.Text.Json;
using Aviator.Airframe.Frames.Strategies.Vdl2.DumpVdl2.Protocol;
using Microsoft.Extensions.Logging;

namespace Aviator.Airframe.Frames.Strategies.AeroL.Jaero.Protocol;

public class JaeroAcarsStrategy(ILogger<DumpVdl2AcarsStrategy> logger) : IJaeroProtocolStrategy
{
    public ProtocolType ProtocolType => ProtocolType.Acars;
    public bool CanHandleProtocol(JsonElement protocol)
    {
        return protocol.TryGetProperty("acars", out var acars) &&
               acars.ValueKind == JsonValueKind.Object;
    }

    public Task<object> HandleProtocolAsync(JsonElement protocol, CancellationToken cancellationToken)
    {
        using var scope = logger.BeginScope(nameof(JaeroAcarsStrategy));
        logger.LogDebug("Handling Acars here");
        
        var hasAcars = protocol.TryGetProperty("acars", out var acars);
        
        var protocolResult = new Entities.Acars
        {
            Label = acars.GetProperty("label").GetString() ?? string.Empty,
            Registration = acars.GetProperty("reg").GetString() ?? string.Empty,
            Text = acars.GetProperty("msg_text").GetString() ?? string.Empty
        };
        
        return Task.FromResult<object>(protocolResult);
    }
}