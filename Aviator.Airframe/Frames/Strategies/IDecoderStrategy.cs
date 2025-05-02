using System.Text.Json;

namespace Aviator.Airframe.Frames.Strategies;

public interface IDecoderStrategy
{
    public FrameType FrameType { get; }
    public bool ThisDecoder(JsonElement acarsFrame);
    public Task HandleAcarsFrame(JsonElement acarsFrame);
}