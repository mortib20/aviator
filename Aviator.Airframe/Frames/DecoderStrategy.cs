using System.Text.Json.Nodes;
using Aviator.Acars.Frames.Strategies;
using Aviator.Acars.Network;
using Microsoft.Extensions.Logging;

namespace Aviator.Acars.Frames;

public class DecoderStrategy(ILogger<DecoderStrategy> logger, Dictionary<DecoderType, IDecoderStrategy> decoderStrategies, IAirframeOutputManager airframeOutputManager)
{
    public async Task HandleAirframeAsync(JsonNode acarsFrame)
    {
        var airframeStrategy = GetAirframeStrategy(acarsFrame);

        if (airframeStrategy is null)
        {
            logger.LogWarning("Strategy for this frame not implemented...");
            return;
        }

        airframeOutputManager.SendToOutputOfTypeAsync(acarsFrame.);
        
        await airframeStrategy.HandleAcarsFrame(acarsFrame).ConfigureAwait(false);
    }

    private IDecoderStrategy? GetAirframeStrategy(JsonNode acarsFrame)
    {
        return decoderStrategies.FirstOrDefault(strategy => strategy.Value.ThisDecoder(acarsFrame)).Value;
    }
}