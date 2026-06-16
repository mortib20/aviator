using System.Text.Json;
using Aviator.Airframe.Frames.Strategies.Xng;
using Microsoft.Extensions.Logging;

namespace Aviator.Airframe.Frames.Strategies.Acars.Xng;

/// <summary>
/// Plain-old ACARS (VHF) decoded by xng (<c>mode == "acars_poa"</c>). The body
/// is always an ACARS message; an ADS-C position is extracted when present.
/// </summary>
public class XngAcarsFrameStrategy(ILogger<XngAcarsFrameStrategy> logger) : IDecoderStrategy
{
    public FrameType FrameType => FrameType.Acars;

    public bool CanHandleAirframe(JsonElement rawAirframe)
    {
        return XngEnvelope.IsXng(rawAirframe) && XngEnvelope.IsMode(rawAirframe, "acars_poa");
    }

    public Task<Entities.Airframe?> HandleAirframeAsync(JsonElement rawAirframe, CancellationToken cancellationToken)
    {
        using var scope = logger.BeginScope(nameof(XngAcarsFrameStrategy));

        if (!XngEnvelope.TryGetBody(rawAirframe, out var body, out var type) || type != "acars")
        {
            logger.LogWarning("xng acars frame did not contain an acars body (type: {Type})", type);
            return Task.FromResult<Entities.Airframe?>(null);
        }

        var acars = XngEnvelope.MapAcars(body);
        var (source, destination) = XngEnvelope.AircraftEndpoints(acars);
        var position = XngEnvelope.AdscPosition(body);

        var airframe = Entities.Airframe.Create(
            FrameType,
            ProtocolType.Acars,
            XngEnvelope.Channel(rawAirframe),
            source,
            destination,
            XngEnvelope.SignalLevel(rawAirframe),
            XngEnvelope.NoiseLevel(rawAirframe),
            acars,
            position,
            icao: acars.Registration);

        return Task.FromResult<Entities.Airframe?>(airframe);
    }
}
