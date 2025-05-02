using System.Text.Json.Nodes;

namespace Aviator.Acars.Frames.Strategies;

public interface IAcarsFrameStrategy
{
    public FrameType FrameType { get; }
    public bool ThisDecoder(JsonNode acarsFrame);
    public Task HandleAcarsFrame(JsonNode acarsFrame);
}