using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Aviator.Airframe.Frames.Strategies.Vdl2.DumpVdl2.Protocol;

public class DumpVdl2X25Strategy(ILogger<DumpVdl2X25Strategy> logger) : IDumpVdl2ProtocolStrategy
{
    public ProtocolType ProtocolType => ProtocolType.X25;
    public bool CanHandleProtocol(JsonElement avlc)
    {
        return
            avlc.TryGetProperty("x25", out var x25)
            && x25.ValueKind == JsonValueKind.Object;
    }

    public Task HandleProtocolAsync(JsonElement avlc, CancellationToken cancellationToken)
    {
        using var scope = logger.BeginScope(nameof(DumpVdl2X25Strategy));
        logger.LogDebug("Handling x25 here");
        
        return Task.CompletedTask;
    }
}