using System.Text.RegularExpressions;
using Aviator.Main.Decoders;

namespace Aviator.Main.Decoders.Metar;

public sealed partial class MetarDecoder : IMessageDecoder
{
    public string Name => "METAR";

    [GeneratedRegex(@"(?:METAR|SPECI)\s+[A-Z]{4}\s+\d{6}Z", RegexOptions.IgnoreCase)]
    private static partial Regex LooksLikeMetarRegex();

    public bool CanDecode(string label, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        if (!text.Contains("METAR", StringComparison.OrdinalIgnoreCase) &&
            !text.Contains("SPECI", StringComparison.OrdinalIgnoreCase))
            return false;
        return LooksLikeMetarRegex().IsMatch(text);
    }

    public IDecodedMessage Decode(string label, string text)
    {
        var reports = MetarParser.ExtractAndParse(text);
        return new MetarDecodedMessage { Reports = reports };
    }
}
