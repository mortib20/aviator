using System.Text.Json.Nodes;
using Aviator.Acars.Frames.Strategies;
using Aviator.Acars.Network;
using Microsoft.Extensions.Logging;

namespace Aviator.Acars.Frames;

public class AcarsFrameHandler(ILogger<AcarsFrameHandler> logger, Dictionary<DecoderType, IAcarsFrameStrategy> acarsFrameStrategies, IAcarsOutputManager acarsOutputManager)
{
    public async Task HandleAcarsFrameAsync(JsonNode acarsFrame)
    {
        var acarsFrameStrategy = GetAcarsFrameStrategy(acarsFrame);

        if (acarsFrameStrategy is null)
        {
            logger.LogWarning("Strategy for this frame not implemented...");
            return;
        }

        acarsOutputManager.SendToOutputOfTypeAsync(acarsFrame.);
        
        await acarsFrameStrategy.HandleAcarsFrame(acarsFrame).ConfigureAwait(false);
    }

    private IAcarsFrameStrategy? GetAcarsFrameStrategy(JsonNode acarsFrame)
    {
        return acarsFrameStrategies.FirstOrDefault(strategy => strategy.Value.ThisDecoder(acarsFrame)).Value;
    }
}