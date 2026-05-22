using System.Text.Json;
using Aviator.Airframe.Frames.Entities;
using Microsoft.Extensions.Logging;

namespace Aviator.Airframe.Frames.Strategies.Iridium.IridiumToolkit;

public class IridiumToolkitFrameStrategy(ILogger<IridiumToolkitFrameStrategy> logger) : IDecoderStrategy
{
    public FrameType FrameType => FrameType.Iridium;
    public bool CanHandleAirframe(JsonElement rawAirframe)
    {
        return rawAirframe.TryGetProperty("app", out var app)
               && app.TryGetProperty("name", out var name)
               && name.ValueKind == JsonValueKind.String
               && name.GetString() is "iridium-toolkit";
    }

    public async Task<Entities.Airframe?> HandleAirframeAsync(JsonElement rawAirframe, CancellationToken cancellationToken)
    {
        using var scope = logger.BeginScope(nameof(IridiumToolkitFrameStrategy));
        logger.LogDebug("Handling Iridium Frame");

        var hasFreq = rawAirframe.TryGetProperty("freq", out var freq);

        if (!hasFreq)
        {
            logger.LogWarning("Frame does not contain frequency...");
            return null;
        }
        
        var signalLevel = rawAirframe.GetProperty("level").GetDouble();
        
        var hasAcars = rawAirframe.TryGetProperty("acars", out var acars);

        if (!hasAcars)
        {
            logger.LogWarning("Frame does not contain acars...");
            return null;
        }
        
        var source = new Source
        {
            Address = acars.GetProperty("tail").GetString() ?? string.Empty,
            SourceType = SourceType.Aircraft
        };
        
        var destination = new Destination
        {
            Address = "000000",
            DestinationType = DestinationType.Unknown
        };
        
        var acarsFrame = new Entities.Acars
        {
            Label = acars.GetProperty("label").GetString() ?? "",
            Registration = acars.GetProperty("tail").GetString() ?? "",
            FlightNumber = acars.TryGetProperty("flight", out var flt) ? flt.GetString() ?? "" : "",
            MessageNumber = acars.TryGetProperty("msgno", out var msgno) ? msgno.GetString() ?? "" : "",
            Text = acars.GetProperty("text").GetString() ?? ""
        };

        var freqStr = freq.ToString();
        var channel = freqStr.Length >= 4 ? freqStr[..4] : freqStr;

        return Entities.Airframe.Create(FrameType, ProtocolType.Acars, channel, source, destination, signalLevel, protocol: acarsFrame);
    }
}