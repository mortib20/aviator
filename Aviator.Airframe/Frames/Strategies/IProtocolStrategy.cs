using System.Text.Json;

namespace Aviator.Airframe.Frames.Strategies;

public interface IProtocolStrategy
{
    ProtocolType ProtocolType { get; }
    bool CanHandleProtocol(JsonElement protocol);
    Task<object> HandleProtocolAsync(JsonElement protocol, CancellationToken cancellationToken);
}