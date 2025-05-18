using System.Globalization;
using System.Text;
using System.Text.Json;
using Aviator.Airframe.Frames.Entities;
using Aviator.Airframe.Frames.Strategies.Vdl2.DumpVdl2.Protocol;
using Microsoft.Extensions.Logging;

namespace Aviator.Airframe.Frames.Strategies.Vdl2.DumpVdl2;

public class DumpVdl2FrameStrategy(ILogger<DumpVdl2FrameStrategy> logger, List<IDumpVdl2ProtocolStrategy> protocolStrategies) : IDecoderStrategy
{
    public FrameType FrameType => FrameType.Vdl2;

    public bool CanHandleAirframe(JsonElement rawAirframe)
    {
        return
            rawAirframe.TryGetProperty("vdl2", out var vdl2)
            && vdl2.TryGetProperty("app", out var app)
            && app.TryGetProperty("name", out var name)
            && name.ValueKind == JsonValueKind.String
            && name.GetString() is "dumpvdl2";
    }

    public async Task<Entities.Airframe?> HandleAirframeAsync(JsonElement rawAirframe, CancellationToken cancellationToken)
    {
        using var scope = logger.BeginScope(nameof(DumpVdl2FrameStrategy));

        var hasVdl2 = rawAirframe.TryGetProperty("vdl2", out var vdl2);

        if (!hasVdl2)
        {
            logger.LogWarning("Frame did not contain Vdl2...");
            return null;
        }

        var hasFreq = vdl2.GetProperty("freq").TryGetDouble(out var freq);
        var hasAvlc = vdl2.TryGetProperty("avlc", out var avlc);

        if (!hasFreq || !hasAvlc)
        {
            logger.LogWarning("Frame did not contain frequency or Avlc...");
            return null;
        }

        var signalLevel = vdl2.GetProperty("sig_level").GetDouble();
        var noiseLevel = vdl2.GetProperty("noise_level").GetDouble();

        var rawSource = avlc.GetProperty("src");
        var source = new Source
        {
            Address = rawSource.GetProperty("addr").GetString() ?? "000000",
            SourceType = rawSource.GetProperty("type").GetString() == "Aircraft" ? SourceType.Aircraft : SourceType.Ground,
        };

        var rawDestination = avlc.GetProperty("dst");
        var destination = new Destination
        {
            Address = rawDestination.GetProperty("addr").GetString() ?? "000000",
            DestinationType = rawDestination.GetProperty("type").GetString() == "Aircraft" ? DestinationType.Aircraft : DestinationType.Ground,
        };

        var protocolStrategy = GetProtocolStrategy(avlc.Clone());

        if (protocolStrategy is null)
        {
            logger.LogWarning("Strategy for this protocol not implemented... {Frame}", Encoding.UTF8.GetString(JsonSerializer.SerializeToUtf8Bytes(rawAirframe)));
            return Entities.Airframe.Create(FrameType, ProtocolType.Unknown, freq.ToString(CultureInfo.InvariantCulture), source, destination, signalLevel, noiseLevel);
        }

        await protocolStrategy.HandleProtocolAsync(avlc, cancellationToken).ConfigureAwait(false);

        return Entities.Airframe.Create(FrameType, protocolStrategy.ProtocolType, freq.ToString(CultureInfo.InvariantCulture), source, destination, signalLevel, noiseLevel);
    }

    private IDumpVdl2ProtocolStrategy? GetProtocolStrategy(JsonElement avlc)
    {
        return protocolStrategies.FirstOrDefault(strategy => strategy.CanHandleProtocol(avlc));
    }
}