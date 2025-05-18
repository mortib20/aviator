using System.Text.Json;

namespace Aviator.Airframe.Frames.Strategies;

public interface IProtocolStrategy
{
    ProtocolType ProtocolType { get; }
    bool CanHandleProtocol(JsonElement avlc);
    Task HandleProtocolAsync(JsonElement avlc, CancellationToken cancellationToken);
}