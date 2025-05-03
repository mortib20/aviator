using System.Text.Json;

namespace Aviator.Airframe.Frames.Strategies.Vdl2.DumpVdl2.Protocol;

public interface IDumpVdl2ProtocolStrategy
{
    public ProtocolType ProtocolType { get; }

    public bool CanHandleProtocol(JsonElement avlc);
    public Task HandleProtocolAsync(JsonElement avlc, CancellationToken cancellationToken);
}