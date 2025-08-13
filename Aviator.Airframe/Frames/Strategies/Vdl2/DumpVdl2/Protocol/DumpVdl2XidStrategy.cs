using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Aviator.Airframe.Frames.Strategies.Vdl2.DumpVdl2.Protocol;

public class DumpVdl2XidStrategy(ILogger<DumpVdl2XidStrategy> logger) : IDumpVdl2ProtocolStrategy
{
    public ProtocolType ProtocolType => ProtocolType.Xid;
    public bool CanHandleProtocol(JsonElement protocol)
    {
        return
            protocol.TryGetProperty("xid", out var xid)
            && xid.ValueKind == JsonValueKind.Object;
    }

    public Task<object> HandleProtocolAsync(JsonElement protocol, CancellationToken cancellationToken)
    {
        using var scope = logger.BeginScope(nameof(DumpVdl2XidStrategy));
        logger.LogDebug("Handling xid here");
        
        return Task.FromResult(new object());
    }
}