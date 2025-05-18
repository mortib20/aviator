using System.Text;
using System.Text.Json;
using Aviator.Airframe.Frames.Strategies;
using Aviator.Airframe.Network;
using Microsoft.Extensions.Logging;

namespace Aviator.Airframe.Frames;

public class AirframeHandler(ILogger<AirframeHandler> logger, ICollection<IDecoderStrategy> decoderStrategies, IAirframeOutputManager airframeOutputManager)
{
    public async Task HandleAirframeAsync(JsonElement rawAirframe, CancellationToken cancellationToken)
    {
        var airframeStrategy = GetAirframeStrategy(rawAirframe);

        if (airframeStrategy is null)
        {
            logger.LogWarning("Strategy for this airframe not implemented...");
            return;
        }
        
        await airframeOutputManager.SendToOutputsOfFrameTypeAsync(airframeStrategy.FrameType, JsonSerializer.SerializeToUtf8Bytes(rawAirframe), cancellationToken).ConfigureAwait(false);
        
        var airframe = await airframeStrategy.HandleAirframeAsync(rawAirframe, cancellationToken).ConfigureAwait(false);

        if (airframe is null)
        {
            logger.LogDebug("Aiframe was null");
            return;
        }
        
        // Metrics
        logger.LogDebug("{Airframe}", airframe);
    }

    private IDecoderStrategy? GetAirframeStrategy(JsonElement acarsFrame)
    {
        return decoderStrategies.FirstOrDefault(strategy => strategy.CanHandleAirframe(acarsFrame));
    }
}