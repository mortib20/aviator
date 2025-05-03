using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Aviator.Airframe.Frames.Strategies.Vdl2.DumpVdl2.Protocol;

public class DumpVdl2AcarsStrategy(ILogger<DumpVdl2AcarsStrategy> logger) : IDumpVdl2ProtocolStrategy
{
    public ProtocolType ProtocolType => ProtocolType.Acars;
    public bool CanHandleProtocol(JsonElement avlc)
    {
        return
            avlc.TryGetProperty("acars", out var acars)
            && acars.ValueKind == JsonValueKind.Object;
    }

    public Task HandleProtocolAsync(JsonElement avlc, CancellationToken cancellationToken)
    {
        using var scope = logger.BeginScope(nameof(DumpVdl2AcarsStrategy));
        logger.LogInformation("Handling ACARS like a pro");

        return Task.CompletedTask;
    }
}