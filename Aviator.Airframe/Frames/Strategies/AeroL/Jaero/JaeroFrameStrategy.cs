using System.Text.Json;
using Aviator.Airframe.Frames.Entities;
using Microsoft.Extensions.Logging;

namespace Aviator.Airframe.Frames.Strategies.AeroL.Jaero;

public class JaeroFrameStrategy(ILogger<JaeroFrameStrategy> logger) : IDecoderStrategy
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
        
        return Entities.Airframe.Create(FrameType, ProtocolType.Unknown, freq.ToString(), source, destination);
    }
}