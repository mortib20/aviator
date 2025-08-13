using System.Text.Json;
using Aviator.Airframe.Frames.Entities;
using Microsoft.Extensions.Logging;

namespace Aviator.Airframe.Frames.Strategies.Acars.Acarsdec;

public class AcarsdecFrameStrategy(ILogger<AcarsdecFrameStrategy> logger) : IDecoderStrategy
{
    public FrameType FrameType => FrameType.Acars;
    public bool CanHandleAirframe(JsonElement rawAirframe)
    {
        return rawAirframe.TryGetProperty("app", out var app)
           && app.TryGetProperty("name", out var name)
           && name.ValueKind == JsonValueKind.String
           && name.GetString() is "acarsdec";
    }

    public async Task<Entities.Airframe?> HandleAirframeAsync(JsonElement rawAirframe, CancellationToken cancellationToken)
    {
        using var scope = logger.BeginScope(nameof(AcarsdecFrameStrategy));
        logger.LogDebug("Handling Acarsdec Frame");

        var hasFreq = rawAirframe.TryGetProperty("freq", out var freq);

        if (!hasFreq)
        {
            logger.LogWarning("Frame does not contain frequency...");
            return null;
        }

        var signalLevel = rawAirframe.GetProperty("level").GetDouble();

        var source = new Source
        {
            Address = rawAirframe.GetProperty("tail").ToString(),
            SourceType = SourceType.Aircraft
        };

        var destination = new Destination
        {
            Address = "000000",
            DestinationType = DestinationType.Unknown
        };

        var acars = new Entities.Acars
        {
            Label = rawAirframe.GetProperty("label").GetString() ?? "",
            Registration = rawAirframe.GetProperty("tail").GetString() ?? "",
            Text = rawAirframe.GetProperty("text").GetString() ?? ""
        };

        return Entities.Airframe.Create(FrameType, ProtocolType.Acars, $"{freq.ToString().Replace(".", "")}000", source, destination, signalLevel: signalLevel, protocol: acars);
    }
}