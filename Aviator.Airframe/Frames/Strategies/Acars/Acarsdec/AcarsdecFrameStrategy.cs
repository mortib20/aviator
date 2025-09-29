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

        var position = TryExtractPositionFromLibacars(rawAirframe);

        return Entities.Airframe.Create(FrameType, ProtocolType.Acars, $"{freq.ToString().Replace(".", "")}000", source, destination, signalLevel: signalLevel, protocol: acars, position: position);
    }
    
    private static Position? TryExtractPositionFromLibacars(JsonElement rawAirframe)
    {
        if (!rawAirframe.TryGetProperty("libacars", out var libacars))
        {
            return null;
        }

        if (!libacars.TryGetProperty("arinc622", out var arinc))
        {
            return null;
        }

        if (!arinc.TryGetProperty("adsc", out var adsc))
        {
            return null;
        }

        if (!adsc.TryGetProperty("tags", out var tags) || tags.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (var tag in tags.EnumerateArray())
        {
            if (tag.TryGetProperty("basic_report", out var basic))
            {
                if (basic.TryGetProperty("lat", out var latProp)
                    && basic.TryGetProperty("lon", out var lonProp)
                    && latProp.TryGetDecimal(out var lat)
                    && lonProp.TryGetDecimal(out var lon))
                {
                    decimal alt = 0;
                    if (basic.TryGetProperty("alt", out var altProp) && altProp.TryGetDecimal(out var parsedAlt))
                    {
                        alt = parsedAlt;
                    }

                    return Position.Create(true, lat, lon, alt);
                }
            }

            // predicted_route.next_wpt oder next_next_wpt
            // if (tag.TryGetProperty("predicted_route", out var route))
            // {
            //     if (route.TryGetProperty("next_wpt", out var wpt))
            //     {
            //         if (wpt.TryGetProperty("lat", out var latProp)
            //             && wpt.TryGetProperty("lon", out var lonProp)
            //             && latProp.TryGetDecimal(out var lat)
            //             && lonProp.TryGetDecimal(out var lon))
            //         {
            //             decimal alt = 0;
            //             if (wpt.TryGetProperty("alt", out var altProp) && altProp.TryGetDecimal(out var parsedAlt))
            //             {
            //                 alt = parsedAlt;
            //             }
            //
            //             return Position.Create(false, lat, lon, alt); // false = predicted
            //         }
            //     }
            // }
        }

        return null;
    }
}