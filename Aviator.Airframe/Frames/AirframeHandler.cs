using System.Text.Json;
using Aviator.Airframe.Frames.Entities;
using Aviator.Airframe.Frames.Strategies;
using Aviator.Airframe.Metrics;
using Aviator.Airframe.Network;
using Aviator.Airframe.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Aviator.Airframe.Frames;

public class AirframeHandler(ILogger<AirframeHandler> logger, ICollection<IDecoderStrategy> decoderStrategies, IAirframeOutputManager airframeOutputManager, AirframeMetrics airframeMetrics, AirframeHub airframeHub)
{
    public async Task HandleAirframeAsync(JsonElement rawAirframe, CancellationToken cancellationToken)
    {
        var airframeStrategy = GetAirframeStrategy(rawAirframe);

        if (airframeStrategy is null)
        {
            logger.LogDebug("Strategy for this airframe not implemented...");
            return;
        }
        
        await airframeOutputManager.SendToOutputsOfFrameTypeAsync(airframeStrategy.FrameType, JsonSerializer.SerializeToUtf8Bytes(rawAirframe), cancellationToken).ConfigureAwait(false);
        
        var airframe = await airframeStrategy.HandleAirframeAsync(rawAirframe, cancellationToken).ConfigureAwait(false);
        
        if (airframe is null)
        {
            logger.LogDebug("Airframe was null");
            return;
        }

        if (airframe.ProtocolType == ProtocolType.Acars)
        {
            var hasVdl2 = rawAirframe.TryGetProperty("vdl2", out var vdl2);
            var hasAvlc = vdl2.TryGetProperty("avlc", out var avlc);
            var hasAcars = avlc.TryGetProperty("acars", out var acars);

            if (hasVdl2 && hasAvlc && !hasAcars)
            {
                airframe.Protocol = new Acars
                {
                    Label = acars.GetProperty("label").GetString() ?? string.Empty,
                    Registration = acars.GetProperty("reg").GetString() ?? string.Empty,
                    Text = acars.GetProperty("msg_text").GetString() ?? string.Empty
                };
            
                await airframeHub.Clients.All.SendAsync("Acars", airframe, cancellationToken).ConfigureAwait(false);   
            }
        }
        
        await airframeMetrics.HandleAirframe(airframe).ConfigureAwait(false);
        
        // Metrics
        logger.LogDebug("{Airframe}", airframe);
    }

    private IDecoderStrategy? GetAirframeStrategy(JsonElement acarsFrame)
    {
        return decoderStrategies.FirstOrDefault(strategy => strategy.CanHandleAirframe(acarsFrame));
    }
}