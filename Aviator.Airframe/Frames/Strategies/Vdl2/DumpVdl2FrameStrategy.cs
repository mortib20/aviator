using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Aviator.Airframe.Frames.Strategies.Vdl2;

public class DumpVdl2FrameStrategy(ILogger<DumpVdl2FrameStrategy> logger) : IDecoderStrategy
{
    public FrameType FrameType => FrameType.Vdl2;

    public bool CanHandleFrame(JsonElement acarsFrame)
    {
        return
            acarsFrame.TryGetProperty("vdl2", out var vdl2)
            && vdl2.TryGetProperty("app", out var app)
            && app.TryGetProperty("name", out var name)
            && name.ValueKind == JsonValueKind.String
            && name.GetString() is "dumpvdl2";
    }

    public Task HandleAcarsFrame(JsonElement acarsFrame)
    {
        logger.LogInformation("I am handling the frame!");
        return Task.CompletedTask;
    }
}