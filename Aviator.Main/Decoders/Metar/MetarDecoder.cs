using System.Text.RegularExpressions;
using Aviator.Main.Decoders;

namespace Aviator.Main.Decoders.Metar;

public sealed partial class MetarDecoder : IMessageDecoder
{
    public string Name => "METAR";

    // Standard METAR: "METAR ICAO DDHHMMz"
    [GeneratedRegex(@"(?:METAR|SPECI)\s+[A-Z]{4}\s+\d{6}Z", RegexOptions.IgnoreCase)]
    private static partial Regex StandardMetarRegex();

    public bool CanDecode(string label, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        if (!text.Contains("METAR", StringComparison.OrdinalIgnoreCase) &&
            !text.Contains("SPECI", StringComparison.OrdinalIgnoreCase))
            return false;
        // Accept both standard METAR format and ATIS-embedded format
        return StandardMetarRegex().IsMatch(text) || AtisParser.LooksLikeAtis(text);
    }

    public IDecodedMessage Decode(string label, string text)
    {
        // Try standard METAR format first
        var reports = MetarParser.ExtractAndParse(text);

        // Fall back to ATIS parser if no standard METARs found
        if (reports.Count == 0 && AtisParser.LooksLikeAtis(text))
        {
            var atisReport = AtisParser.Parse(text);
            if (atisReport is not null)
                reports = [atisReport];
        }

        return new MetarDecodedMessage { Reports = reports };
    }
}
