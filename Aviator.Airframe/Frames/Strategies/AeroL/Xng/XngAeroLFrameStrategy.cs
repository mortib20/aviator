using System.Text.Json;
using Aviator.Airframe.Frames.Strategies.Xng;
using Microsoft.Extensions.Logging;

namespace Aviator.Airframe.Frames.Strategies.AeroL.Xng;

/// <summary>
/// Inmarsat Classic Aero L-band decoded by xng (<c>mode == "aero_l"</c>).
/// ACARS carried over the satellite link surfaces as an <c>acars</c> body
/// (with ADS-C position where present); C-channel assignments and other
/// non-ACARS structures are passed through as unknown protocol.
/// </summary>
public class XngAeroLFrameStrategy(ILogger<XngAeroLFrameStrategy> logger) : IDecoderStrategy
{
    public FrameType FrameType => FrameType.AeroL;

    public bool CanHandleAirframe(JsonElement rawAirframe)
    {
        return XngEnvelope.IsXng(rawAirframe) && XngEnvelope.IsMode(rawAirframe, "aero_l");
    }

    public Task<Entities.Airframe?> HandleAirframeAsync(JsonElement rawAirframe, CancellationToken cancellationToken)
    {
        using var scope = logger.BeginScope(nameof(XngAeroLFrameStrategy));

        if (!XngEnvelope.TryGetBody(rawAirframe, out var body, out var type))
        {
            logger.LogWarning("xng aero-l frame did not contain a body");
            return Task.FromResult<Entities.Airframe?>(null);
        }

        var channel = XngEnvelope.Channel(rawAirframe);
        var signalLevel = XngEnvelope.SignalLevel(rawAirframe);
        var noiseLevel = XngEnvelope.NoiseLevel(rawAirframe);

        if (type != "acars")
        {
            logger.LogDebug("Aero-L body type {Type} has no protocol mapping", type);
            var (unknownSource, unknownDestination) = XngEnvelope.UnknownEndpoints();
            return Task.FromResult<Entities.Airframe?>(Entities.Airframe.Create(
                FrameType,
                ProtocolType.Unknown,
                channel,
                unknownSource,
                unknownDestination,
                signalLevel,
                noiseLevel));
        }

        var acars = XngEnvelope.MapAcars(body);
        var (source, destination) = XngEnvelope.AircraftEndpoints(acars);
        var position = XngEnvelope.AdscPosition(body);

        return Task.FromResult<Entities.Airframe?>(Entities.Airframe.Create(
            FrameType,
            ProtocolType.Acars,
            channel,
            source,
            destination,
            signalLevel,
            noiseLevel,
            acars,
            position,
            icao: acars.Registration));
    }
}
