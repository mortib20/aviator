using System.Text.Json;

namespace Aviator.Airframe.Frames.Strategies;

public interface IDecoderStrategy
{
    public FrameType FrameType { get; }
    public bool CanHandleAirframe(JsonElement rawAirframe);
    public Task<Entities.Airframe?> HandleAirframeAsync(JsonElement rawAirframe, CancellationToken cancellationToken);
}