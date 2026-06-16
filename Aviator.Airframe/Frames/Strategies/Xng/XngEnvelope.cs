using System.Globalization;
using System.Text.Json;
using Aviator.Airframe.Frames.Entities;

namespace Aviator.Airframe.Frames.Strategies.Xng;

/// <summary>
/// Shared parsing for the unified xng JSON envelope. Every xng message shares
/// the same root shape (<c>mode</c>, <c>signal</c>, <c>decode</c>, <c>body</c>,
/// <c>source</c>); the per-mode <see cref="IDecoderStrategy"/> implementations
/// only differ in which <c>mode</c> they accept and how they treat the body.
/// </summary>
internal static class XngEnvelope
{
    /// <summary>True when the frame was produced by xng (<c>source.app.name == "xng"</c>).</summary>
    public static bool IsXng(JsonElement root)
    {
        return root.TryGetProperty("source", out var source)
               && source.TryGetProperty("app", out var app)
               && app.TryGetProperty("name", out var name)
               && name.ValueKind == JsonValueKind.String
               && name.GetString() == "xng";
    }

    /// <summary>True when the frame's <c>mode</c> matches (e.g. <c>vdl2</c>, <c>acars_poa</c>).</summary>
    public static bool IsMode(JsonElement root, string mode)
    {
        return root.TryGetProperty("mode", out var modeEl)
               && modeEl.ValueKind == JsonValueKind.String
               && modeEl.GetString() == mode;
    }

    /// <summary>The tagged message body and its <c>type</c> discriminator (acars, vdl2, iridium, ...).</summary>
    public static bool TryGetBody(JsonElement root, out JsonElement body, out string type)
    {
        type = string.Empty;
        if (!root.TryGetProperty("body", out body) || body.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (body.TryGetProperty("type", out var typeEl) && typeEl.ValueKind == JsonValueKind.String)
        {
            type = typeEl.GetString() ?? string.Empty;
        }

        return true;
    }

    /// <summary>Carrier frequency in Hz as a string, used as the channel identifier.</summary>
    public static string Channel(JsonElement root)
    {
        return root.TryGetProperty("frequency_hz", out var freq) && freq.TryGetUInt64(out var hz)
            ? hz.ToString(CultureInfo.InvariantCulture)
            : string.Empty;
    }

    /// <summary>Received signal strength (dBFS) from <c>signal.rssi_db</c>.</summary>
    public static double? SignalLevel(JsonElement root)
    {
        return TryGetSignal(root, "rssi_db");
    }

    /// <summary>Noise floor estimate (dBFS) from <c>signal.noise_db</c>.</summary>
    public static double? NoiseLevel(JsonElement root)
    {
        return TryGetSignal(root, "noise_db");
    }

    private static double? TryGetSignal(JsonElement root, string property)
    {
        return root.TryGetProperty("signal", out var signal)
               && signal.TryGetProperty(property, out var value)
               && value.ValueKind == JsonValueKind.Number
               && value.TryGetDouble(out var d)
            ? d
            : null;
    }

    /// <summary>
    /// Map an xng <c>acars</c> body to the domain <see cref="Entities.Acars"/>.
    /// xng follows ARINC 618 naming; the AVLC/satellite link addresses are not
    /// carried in the ACARS body, so the registration (<c>tail</c>) is the only
    /// aircraft identifier available here.
    /// </summary>
    public static Entities.Acars MapAcars(JsonElement body)
    {
        return new Entities.Acars
        {
            Label = GetString(body, "label"),
            Registration = GetString(body, "tail"),
            FlightNumber = GetString(body, "flight"),
            MessageNumber = GetString(body, "msg_num"),
            Text = GetString(body, "text"),
        };
    }

    /// <summary>
    /// Extract an aircraft position from the decoded application layer. xng
    /// surfaces ADS-C under <c>body.app</c> as <c>{ "app": "adsc", "tags": [ ... ] }</c>;
    /// a basic report tag carries <c>lat</c>/<c>lon</c>/<c>alt_ft</c>.
    /// </summary>
    public static Position? AdscPosition(JsonElement body)
    {
        if (!body.TryGetProperty("app", out var app) || app.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (!app.TryGetProperty("app", out var appName)
            || appName.GetString() != "adsc"
            || !app.TryGetProperty("tags", out var tags)
            || tags.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (var tag in tags.EnumerateArray())
        {
            if (tag.TryGetProperty("tag", out var tagName)
                && tagName.GetString() == "report"
                && tag.TryGetProperty("lat", out var latEl)
                && tag.TryGetProperty("lon", out var lonEl)
                && latEl.TryGetDecimal(out var lat)
                && lonEl.TryGetDecimal(out var lon))
            {
                decimal alt = 0;
                if (tag.TryGetProperty("alt_ft", out var altEl) && altEl.TryGetDecimal(out var parsedAlt))
                {
                    alt = parsedAlt;
                }

                return Position.Create(true, lat, lon, alt);
            }
        }

        return null;
    }

    /// <summary>
    /// A source/destination pair for ACARS bodies. The link-layer addresses are
    /// not present in the xng ACARS body, so the aircraft is identified by its
    /// registration and the destination is left unknown (matching the existing
    /// acarsdec/iridium-toolkit behaviour).
    /// </summary>
    public static (Source source, Destination destination) AircraftEndpoints(Entities.Acars acars)
    {
        var source = new Source
        {
            Address = string.IsNullOrEmpty(acars.Registration) ? "000000" : acars.Registration,
            SourceType = SourceType.Aircraft,
        };

        var destination = new Destination
        {
            Address = "000000",
            DestinationType = DestinationType.Unknown,
        };

        return (source, destination);
    }

    /// <summary>A source/destination pair with both endpoints unknown, for non-ACARS bodies without addressing.</summary>
    public static (Source source, Destination destination) UnknownEndpoints()
    {
        var source = new Source { Address = "000000", SourceType = SourceType.Unknown };
        var destination = new Destination { Address = "000000", DestinationType = DestinationType.Unknown };
        return (source, destination);
    }

    private static string GetString(JsonElement element, string property)
    {
        return element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;
    }
}
