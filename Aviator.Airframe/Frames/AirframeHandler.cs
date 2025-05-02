using System.Text.Json;
using Aviator.Airframe.Frames.Strategies;
using Aviator.Airframe.Network;
using Microsoft.Extensions.Logging;

namespace Aviator.Airframe.Frames;

public class AirframeHandler(ILogger<AirframeHandler> logger, Dictionary<DecoderType, IDecoderStrategy> decoderStrategies, IAirframeOutputManager airframeOutputManager)
{
    public async Task HandleAirframeAsync(JsonElement airframe, CancellationToken cancellationToken)
    {
        var airframeStrategy = GetAirframeStrategy(airframe);

        if (airframeStrategy is null)
        {
            logger.LogWarning("Strategy for this frame not implemented...");
            return;
        }
        
        await airframeOutputManager.SendToOutputsOfFrameTypeAsync(airframeStrategy.FrameType, JsonSerializer.SerializeToUtf8Bytes(airframe),cancellationToken).ConfigureAwait(false);
        
        await airframeStrategy.HandleAcarsFrame(airframe).ConfigureAwait(false);
    }

    private IDecoderStrategy? GetAirframeStrategy(JsonElement acarsFrame)
    {
        return decoderStrategies.FirstOrDefault(strategy => strategy.Value.ThisDecoder(acarsFrame)).Value;
    }
}