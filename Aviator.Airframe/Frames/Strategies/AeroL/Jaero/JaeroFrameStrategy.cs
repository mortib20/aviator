using System.Text;
using System.Text.Json;
using Aviator.Airframe.Frames.Entities;
using Aviator.Airframe.Frames.Strategies.AeroL.Jaero.Protocol;
using Microsoft.Extensions.Logging;

namespace Aviator.Airframe.Frames.Strategies.AeroL.Jaero;

public class JaeroFrameStrategy(ILogger<JaeroFrameStrategy> logger, List<IJaeroProtocolStrategy> protocolStrategies) : IDecoderStrategy
{
    public FrameType FrameType => FrameType.AeroL;
    public bool CanHandleAirframe(JsonElement rawAirframe)
    {
        return rawAirframe.TryGetProperty("app", out var app)
               && app.TryGetProperty("name", out var name)
               && name.ValueKind == JsonValueKind.String
               && name.GetString() == "JAERO";
    }

    public async Task<Entities.Airframe?> HandleAirframeAsync(JsonElement rawAirframe, CancellationToken cancellationToken)
    {
        using var scope = logger.BeginScope(nameof(JaeroFrameStrategy));

        var hasIsu = rawAirframe.TryGetProperty("isu", out var isu);
        var hasFreq = rawAirframe.GetProperty("app").TryGetProperty("ver", out var freq);
        
        if (!hasIsu || !hasFreq)
        {
            logger.LogWarning("Frame did not contain frequency or Isu...");
            return null;
        }
        
        var rawSource = isu.GetProperty("src");
        var source = new Source
        {
            Address = rawSource.GetProperty("addr").GetString() ?? "000000",
            SourceType = rawSource.GetProperty("type").GetString() == "Aircraft" ? SourceType.Aircraft : SourceType.Ground,
        };

        var rawDestination = isu.GetProperty("dst");
        var destination = new Destination
        {
            Address = rawDestination.GetProperty("addr").GetString() ?? "000000",
            DestinationType = rawDestination.GetProperty("type").GetString() == "Aircraft" ? DestinationType.Aircraft : DestinationType.Ground,
        };
        
        var protocolStrategy = GetProtocolStrategy(isu.Clone());
        
        if (protocolStrategy is null)
        {
            logger.LogDebug("Strategy for this protocol not implemented... {Frame}", Encoding.UTF8.GetString(JsonSerializer.SerializeToUtf8Bytes(rawAirframe)));
            return Entities.Airframe.Create(FrameType, ProtocolType.Unknown, freq.ToString(), source, destination);
        }

        var protocol = await protocolStrategy.HandleProtocolAsync(isu, cancellationToken).ConfigureAwait(false);
        
        return Entities.Airframe.Create(FrameType, protocolStrategy.ProtocolType, freq.ToString(), source, destination, protocol: protocol);
    }
    
    private IJaeroProtocolStrategy? GetProtocolStrategy(JsonElement avlc)
    {
        return protocolStrategies.FirstOrDefault(strategy => strategy.CanHandleProtocol(avlc));
    }
}