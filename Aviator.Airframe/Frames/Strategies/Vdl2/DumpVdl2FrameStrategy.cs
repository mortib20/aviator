using System.Text.Json.Nodes;

namespace Aviator.Acars.Frames.Strategies.Vdl2;

public class DumpVdl2FrameStrategy() : IAcarsFrameStrategy
{
    public FrameType FrameType => FrameType.Vdl2;

    public bool ThisDecoder(JsonNode acarsFrame)
    {
        return acarsFrame?["vdl2"]?["app"]?["name"]?.ToString() == "dumpvdl2";
    }

    public Task HandleAcarsFrame(JsonNode acarsFrame)
    {
        return Task.CompletedTask;
    }
}