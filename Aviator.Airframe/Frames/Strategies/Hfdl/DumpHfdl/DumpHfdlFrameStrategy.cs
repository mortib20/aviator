using System.Globalization;
using System.Text.Json;
using Aviator.Airframe.Frames.Entities;
using Aviator.Airframe.Frames.Strategies.Hfdl.DumpHfdl.Protocol;
using Aviator.Airframe.Frames.Strategies.Vdl2.DumpVdl2;
using Microsoft.Extensions.Logging;

namespace Aviator.Airframe.Frames.Strategies.Hfdl.DumpHfdl;

public class DumpHfdlFrameStrategy(ILogger<DumpVdl2FrameStrategy> logger, List<IDumpHfdlProtocolStrategy> protocolStrategies) : IDecoderStrategy
{
    public FrameType FrameType => FrameType.Hfdl;

    public bool CanHandleAirframe(JsonElement rawAirframe)
    {
        return rawAirframe.TryGetProperty("hfdl", out var hfdl)
               && hfdl.TryGetProperty("app", out var app)
               && app.TryGetProperty("name", out var name)
               && name.ValueKind == JsonValueKind.String
               && name.GetString() is "dumphfdl";
    }

    public async Task<Entities.Airframe?> HandleAirframeAsync(JsonElement rawAirframe, CancellationToken cancellationToken)
    {
        using var scope = logger.BeginScope(nameof(DumpHfdlFrameStrategy));

        var hasHfdl = rawAirframe.TryGetProperty("hfdl", out var hfdl);

        if (!hasHfdl)
        {
            logger.LogWarning("Frame did not contain Hfdl...");
            return null;
        }

        var hasFreq = hfdl.GetProperty("freq").TryGetDouble(out var freq);
        var hasLpdu = hfdl.TryGetProperty("lpdu", out var lpdu);
        var hasSpdu = hfdl.TryGetProperty("spdu", out var spdu);

        if (!hasFreq)
        {
            logger.LogWarning("Frame did not contain frequency... {Frame}", rawAirframe.ToString());
            return null;
        }

        if (hasSpdu)
        {
            // TODO implement something
            logger.LogDebug("Frame contained spdu, currently not implemented...");
            return null;
        }

        if (!hasLpdu)
        {
            logger.LogWarning("Frame did not contain lpdu... {Frame}", rawAirframe.ToString());
            return null;
        }

        var signalLevel = hfdl.GetProperty("sig_level").GetDouble();
        var noiseLevel = hfdl.GetProperty("noise_level").GetDouble();

        var rawSource = lpdu.GetProperty("src");
        var source = new Source
        {
            Address = rawSource.GetProperty("id").GetInt32().ToString(),
            SourceType = rawSource.GetProperty("type").GetString() == "Aircraft" ? SourceType.Aircraft : SourceType.Ground
        };

        var rawDestination = lpdu.GetProperty("dst");
        var destination = new Destination()
        {
            Address = rawDestination.GetProperty("id").GetInt32().ToString(),
            DestinationType = rawDestination.GetProperty("type").GetString() == "Aircraft" ? DestinationType.Aircraft : DestinationType.Ground
        };

        // ICAO
        JsonElement lpduAcInfoIcao = default;
        JsonElement sourceAcInfoIcao = default;

        var lpduAcInfoHasIcao =
            lpdu.TryGetProperty("ac_info", out var acInfo) &&
            acInfo.TryGetProperty("icao", out lpduAcInfoIcao) &&
            lpduAcInfoIcao.ValueKind == JsonValueKind.String;

        var sourceAcInfoHasIcao =
            rawSource.TryGetProperty("ac_info", out var sourceAcInfo) &&
            sourceAcInfo.TryGetProperty("icao", out sourceAcInfoIcao) &&
            sourceAcInfoIcao.ValueKind == JsonValueKind.String;

        var icao = lpduAcInfoHasIcao
            ? lpduAcInfoIcao.ToString()
            : (sourceAcInfoHasIcao ? sourceAcInfoIcao.ToString() : null);
        // ICAO End
        
        // HFNPDU
        var hasHfnpdu = lpdu.TryGetProperty("hfnpdu", out var hfnpdu);
        // HFNPDU End
        // Position
        JsonElement pos = default;
        var hasPosition = hasHfnpdu && hfnpdu.TryGetProperty("pos", out pos);
        var position = hasHfnpdu && hasPosition &&
                       pos.TryGetProperty("lat", out var lat) &&
                       pos.TryGetProperty("lon", out var lon) &&
                       lat.GetDecimal() < 180 &&
                       lon.GetDecimal() < 180
            ? Position.Create(true, lat.GetDecimal(), lon.GetDecimal(), -1)
            : null;
        
        // Position End

        var protocolStrategy = GetProtocolStrategy(lpdu.Clone());
        
        if (protocolStrategy is null)
        {
            logger.LogDebug("Strategy for this protocol not implemented...");
            return Entities.Airframe.Create(FrameType, ProtocolType.Unknown, freq.ToString(CultureInfo.InvariantCulture), source, destination, signalLevel, noiseLevel, position: position, icao: icao);    
        }

        var protocol = await protocolStrategy.HandleProtocolAsync(lpdu, cancellationToken);
        
        return Entities.Airframe.Create(FrameType, protocolStrategy.ProtocolType, freq.ToString(CultureInfo.InvariantCulture), source, destination, signalLevel, noiseLevel, protocol, position, icao);
    }

    private IDumpHfdlProtocolStrategy? GetProtocolStrategy(JsonElement lpdu)
    {
        return protocolStrategies.FirstOrDefault(strategy => strategy.CanHandleProtocol(lpdu));
    }
}