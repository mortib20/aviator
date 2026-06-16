using System.Text.Json;
using Aviator.Airframe.Frames.Entities;
using Aviator.Airframe.Frames.Strategies.Xng;
using Microsoft.Extensions.Logging;

namespace Aviator.Airframe.Frames.Strategies.Vdl2.Xng;

/// <summary>
/// VDL Mode 2 decoded by xng (<c>mode == "vdl2"</c>). ACARS over AVLC surfaces
/// as an <c>acars</c> body; AVLC link events (XID handoffs, ATN/X.25 traffic)
/// surface as a <c>vdl2</c> body that still carries the decoded link addresses.
/// </summary>
public class XngVdl2FrameStrategy(ILogger<XngVdl2FrameStrategy> logger) : IDecoderStrategy
{
    public FrameType FrameType => FrameType.Vdl2;

    public bool CanHandleAirframe(JsonElement rawAirframe)
    {
        return XngEnvelope.IsXng(rawAirframe) && XngEnvelope.IsMode(rawAirframe, "vdl2");
    }

    public Task<Entities.Airframe?> HandleAirframeAsync(JsonElement rawAirframe, CancellationToken cancellationToken)
    {
        using var scope = logger.BeginScope(nameof(XngVdl2FrameStrategy));

        if (!XngEnvelope.TryGetBody(rawAirframe, out var body, out var type))
        {
            logger.LogWarning("xng vdl2 frame did not contain a body");
            return Task.FromResult<Entities.Airframe?>(null);
        }

        var channel = XngEnvelope.Channel(rawAirframe);
        var signalLevel = XngEnvelope.SignalLevel(rawAirframe);
        var noiseLevel = XngEnvelope.NoiseLevel(rawAirframe);

        if (type == "acars")
        {
            var acars = XngEnvelope.MapAcars(body);
            var (acarsSource, acarsDestination) = XngEnvelope.AircraftEndpoints(acars);
            var acarsPosition = XngEnvelope.AdscPosition(body);

            return Task.FromResult<Entities.Airframe?>(Entities.Airframe.Create(
                FrameType,
                ProtocolType.Acars,
                channel,
                acarsSource,
                acarsDestination,
                signalLevel,
                noiseLevel,
                acars,
                acarsPosition,
                icao: acars.Registration));
        }

        // Non-ACARS AVLC frame: addresses are decoded in the body details.
        var (srcAddr, srcIsAircraft) = ReadAddress(body, "src");
        var (dstAddr, dstIsAircraft) = ReadAddress(body, "dst");

        var source = new Source
        {
            Address = srcAddr,
            SourceType = srcIsAircraft ? SourceType.Aircraft : SourceType.Ground,
        };
        var destination = new Destination
        {
            Address = dstAddr,
            DestinationType = dstIsAircraft ? DestinationType.Aircraft : DestinationType.Ground,
        };
        var icao = srcIsAircraft ? srcAddr : dstAddr;

        return Task.FromResult<Entities.Airframe?>(Entities.Airframe.Create(
            FrameType,
            MapProtocolType(type),
            channel,
            source,
            destination,
            signalLevel,
            noiseLevel,
            icao: icao));
    }

    private static ProtocolType MapProtocolType(string bodyType)
    {
        return bodyType switch
        {
            "xid" => ProtocolType.Xid,
            "atn" => ProtocolType.X25,
            _ => ProtocolType.Unknown,
        };
    }

    private static (string address, bool isAircraft) ReadAddress(JsonElement body, string property)
    {
        if (body.TryGetProperty(property, out var addr) && addr.ValueKind == JsonValueKind.Object)
        {
            var isAircraft = addr.TryGetProperty("kind", out var kind) && kind.GetString() == "aircraft";
            var address = addr.TryGetProperty("addr", out var a) ? a.GetString() ?? "000000" : "000000";
            return (address, isAircraft);
        }

        return ("000000", false);
    }
}
