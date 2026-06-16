using System.Text.Json;
using Aviator.Airframe.Frames.Strategies.Xng;
using Microsoft.Extensions.Logging;

namespace Aviator.Airframe.Frames.Strategies.Iridium.Xng;

/// <summary>
/// Iridium decoded by xng (<c>mode == "iridium"</c>). SBD-carried ACARS surfaces
/// as an <c>acars</c> body and is treated as ACARS traffic; all other Iridium
/// frames (ring alert, broadcast, ...) are passed through as unknown protocol.
/// </summary>
public class XngIridiumFrameStrategy(ILogger<XngIridiumFrameStrategy> logger) : IDecoderStrategy
{
    public FrameType FrameType => FrameType.Iridium;

    public bool CanHandleAirframe(JsonElement rawAirframe)
    {
        return XngEnvelope.IsXng(rawAirframe) && XngEnvelope.IsMode(rawAirframe, "iridium");
    }

    public Task<Entities.Airframe?> HandleAirframeAsync(JsonElement rawAirframe, CancellationToken cancellationToken)
    {
        using var scope = logger.BeginScope(nameof(XngIridiumFrameStrategy));

        if (!XngEnvelope.TryGetBody(rawAirframe, out var body, out var type))
        {
            logger.LogWarning("xng iridium frame did not contain a body");
            return Task.FromResult<Entities.Airframe?>(null);
        }

        var channel = XngEnvelope.Channel(rawAirframe);
        var signalLevel = XngEnvelope.SignalLevel(rawAirframe);
        var noiseLevel = XngEnvelope.NoiseLevel(rawAirframe);

        if (type != "acars")
        {
            logger.LogDebug("Iridium body type {Type} has no protocol mapping", type);
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
