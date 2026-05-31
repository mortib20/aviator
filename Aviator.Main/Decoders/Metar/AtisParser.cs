using System.Text.RegularExpressions;

namespace Aviator.Main.Decoders.Metar;

/// <summary>
/// Parses ATIS-embedded weather from ACARS labels A9 / HX.
/// Format example:
///   ATIS EDDB M METAR 311020
///   30012KT 270V340
///   CAVOK
///   T23 DP11
///   QNH1013
///   TREND NOSIG
/// </summary>
public static partial class AtisParser
{
    [GeneratedRegex(@"\bATIS\s+(?<icao>[A-Z]{4})\b", RegexOptions.IgnoreCase)]
    private static partial Regex AtisIcaoRegex();

    // ATIS time: standalone HHMMz or DDHHMMz
    [GeneratedRegex(@"\b(\d{4}|\d{6})Z\b", RegexOptions.IgnoreCase)]
    private static partial Regex AtisTimeRegex();

    [GeneratedRegex(@"^(?<dir>(?:\d{3}|VRB))(?<speed>\d{2,3})(?:G(?<gust>\d{2,3}))?(?<unit>KT|MPS|KMH)$", RegexOptions.IgnoreCase)]
    private static partial Regex WindRegex();

    // ATIS temp: T23 or TM05  (M = minus)
    [GeneratedRegex(@"^T(?<neg>M?)(?<val>\d{1,2})$", RegexOptions.IgnoreCase)]
    private static partial Regex AtisTempRegex();

    // ATIS dew point: DP11 or DPM02
    [GeneratedRegex(@"^DP(?<neg>M?)(?<val>\d{1,2})$", RegexOptions.IgnoreCase)]
    private static partial Regex AtisDewRegex();

    // ATIS QNH: QNH1013
    [GeneratedRegex(@"^QNH(?<val>\d{3,4})$", RegexOptions.IgnoreCase)]
    private static partial Regex AtisQnhRegex();

    // 4-digit visibility
    [GeneratedRegex(@"^\d{4}$")]
    private static partial Regex VisRegex();

    // Variable wind sector: 200V300
    [GeneratedRegex(@"^\d{3}V\d{3}$")]
    private static partial Regex VarWindRegex();

    public static bool LooksLikeAtis(string text) =>
        AtisIcaoRegex().IsMatch(text);

    public static MetarReport? Parse(string text)
    {
        var icaoMatch = AtisIcaoRegex().Match(text);
        if (!icaoMatch.Success) return null;
        var station = icaoMatch.Groups["icao"].Value.ToUpperInvariant();

        // Time
        DateTime? obsTime = null;
        var timeMatch = AtisTimeRegex().Match(text);
        if (timeMatch.Success)
        {
            var ts = timeMatch.Groups[1].Value;
            obsTime = ts.Length == 6
                ? ParseTime6(ts)
                : ParseTime4(ts);
        }

        // Tokenise the whole text — scan greedily for weather elements
        var tokens = Regex.Replace(text, @"\s+", " ")
                          .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        MetarWind? wind = null;
        int? visMeters = null;
        bool cavok = false;
        double? temp = null, dew = null;
        int? qnh = null;
        var trendParts = new List<string>();
        bool inTrend = false;

        foreach (var tok in tokens)
        {
            if (tok.Equals("CAVOK", StringComparison.OrdinalIgnoreCase))
            {
                cavok = true;
                visMeters = 10000;
                continue;
            }

            if (tok.Equals("NOSIG", StringComparison.OrdinalIgnoreCase) ||
                tok.Equals("TREND", StringComparison.OrdinalIgnoreCase) ||
                tok.Equals("TEMPO", StringComparison.OrdinalIgnoreCase) ||
                tok.Equals("BECMG", StringComparison.OrdinalIgnoreCase))
            {
                inTrend = true;
                trendParts.Add(tok);
                continue;
            }

            if (inTrend) { trendParts.Add(tok); continue; }

            if (wind is null && WindRegex().IsMatch(tok))
            {
                wind = ParseWind(tok);
                continue;
            }

            // Skip variable wind sector
            if (VarWindRegex().IsMatch(tok)) continue;

            var tm = AtisTempRegex().Match(tok);
            if (tm.Success)
            {
                var v = int.Parse(tm.Groups["val"].Value);
                temp = tm.Groups["neg"].Value.Length > 0 ? -v : v;
                continue;
            }

            var dm = AtisDewRegex().Match(tok);
            if (dm.Success)
            {
                var v = int.Parse(dm.Groups["val"].Value);
                dew = dm.Groups["neg"].Value.Length > 0 ? -v : v;
                continue;
            }

            var qm = AtisQnhRegex().Match(tok);
            if (qm.Success)
            {
                qnh = int.Parse(qm.Groups["val"].Value);
                continue;
            }

            if (!cavok && visMeters is null && VisRegex().IsMatch(tok))
            {
                if (int.TryParse(tok, out var v) && v >= 100)
                    visMeters = v;
                continue;
            }
        }

        if (wind is null && temp is null && qnh is null) return null;

        return new MetarReport
        {
            RawText = text.Trim(),
            IsSpeci = false,
            StationIcao = station,
            ObservationTime = obsTime,
            Wind = wind,
            VisibilityMeters = visMeters,
            Cavok = cavok,
            Clouds = [],
            WeatherGroups = [],
            Temperature = temp,
            DewPoint = dew,
            Qnh = qnh,
            Trend = trendParts.Count > 0 ? string.Join(" ", trendParts) : null
        };
    }

    private static MetarWind? ParseWind(string token)
    {
        var m = WindRegex().Match(token);
        if (!m.Success) return null;
        int? dir = m.Groups["dir"].Value.Equals("VRB", StringComparison.OrdinalIgnoreCase)
            ? null : int.Parse(m.Groups["dir"].Value);
        var speed = int.Parse(m.Groups["speed"].Value);
        int? gust = m.Groups["gust"].Success ? int.Parse(m.Groups["gust"].Value) : null;
        var unit = m.Groups["unit"].Value.ToUpperInvariant();
        if (unit == "MPS") { speed = (int)Math.Round(speed * 1.94384); if (gust.HasValue) gust = (int)Math.Round(gust.Value * 1.94384); }
        else if (unit == "KMH") { speed = (int)Math.Round(speed * 0.539957); if (gust.HasValue) gust = (int)Math.Round(gust.Value * 0.539957); }
        return new MetarWind { DirectionDegrees = dir, SpeedKnots = speed, GustKnots = gust };
    }

    private static DateTime? ParseTime4(string hhmm)
    {
        if (!int.TryParse(hhmm[..2], out var h) || !int.TryParse(hhmm[2..], out var m)) return null;
        var now = DateTime.UtcNow;
        try { return new DateTime(now.Year, now.Month, now.Day, h, m, 0, DateTimeKind.Utc); }
        catch { return null; }
    }

    private static DateTime? ParseTime6(string ddhhmmm)
    {
        if (!int.TryParse(ddhhmmm[..2], out var day)) return null;
        if (!int.TryParse(ddhhmmm[2..4], out var h)) return null;
        if (!int.TryParse(ddhhmmm[4..6], out var m)) return null;
        var now = DateTime.UtcNow;
        try { return new DateTime(now.Year, now.Month, day, h, m, 0, DateTimeKind.Utc); }
        catch { return null; }
    }
}
