using System.Text.Json;
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

    public Task<Entities.Airframe?> HandleAirframeAsync(JsonElement rawAirframe, CancellationToken cancellationToken)
    {
        using var scope = logger.BeginScope(nameof(AcarsdecFrameStrategy));
        logger.LogDebug("Handling Acarsdec Frame");

        // TODO add more
        
        return Task.FromResult<Entities.Airframe?>(null);
    }
}