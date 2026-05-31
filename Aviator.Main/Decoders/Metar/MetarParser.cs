using System.Text.RegularExpressions;

namespace Aviator.Main.Decoders.Metar;

public static partial class MetarParser
{
    // Terminated METAR block: from keyword to = (handles multi-line)
    [GeneratedRegex(@"(?:METAR|SPECI)\b[^=]+=", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex MetarTerminatedRegex();

    // Single-line METAR without terminator
    [GeneratedRegex(@"(?:METAR|SPECI)\s+[A-Z]{4}\s+\d{6}Z[^\r\n]*", RegexOptions.IgnoreCase)]
    private static partial Regex MetarLineRegex();

    [GeneratedRegex(@"^(?<dir>(?:\d{3}|VRB))(?<speed>\d{2,3})(?:G(?<gust>\d{2,3}))?(?<unit>KT|MPS|KMH)$", RegexOptions.IgnoreCase)]
    private static partial Regex WindRegex();

    [GeneratedRegex(@"^\d{3}V\d{3}$")]
    private static partial Regex VarWindSectorRegex();

    [GeneratedRegex(@"^R\d{2}[LCR]?/[MP]?\d{4}[UDN]?(/[MP]?\d{4}[UDN]?)?$")]
    private static partial Regex RvrRegex();

    [GeneratedRegex(@"^(?:M?\d{1,2}|\/\/)\/(?:M?\d{1,2}|\/\/)$")]
    private static partial Regex TempDewRegex();

    // Cloud groups — allow trailing /// (automated stations: cloud type not observed)
    [GeneratedRegex(@"^(?<cov>FEW|SCT|BKN|OVC|VV)(?<alt>\d{3})(?:(?<type>CB|TCU)|///)?$", RegexOptions.IgnoreCase)]
    private static partial Regex CloudRegex();

    // Present weather: optional intensity (+/-/VC), optional descriptor, one or more phenomena
    [GeneratedRegex(@"^([+\-]|VC)?(?:MI|BC|PR|DR|BL|SH|TS|FZ)?(?:DZ|RA|SN|SG|IC|PL|GR|GS|UP|BR|FG|FU|VA|DU|SA|HZ|PO|SQ|FC|SS|DS)+$")]
    private static partial Regex WeatherRegex();

    // Directional minimum visibility: 1200SW, 0400N
    [GeneratedRegex(@"^\d{4}[NSEW]{1,2}$")]
    private static partial Regex DirVisRegex();

    public static List<MetarReport> ExtractAndParse(string text)
    {
        var reports = new List<MetarReport>();
        // Keyed by station ICAO so we keep only the first (or best) parse per station
        var seenRaw  = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenIcao = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void TryAdd(string raw)
        {
            raw = NormalizeWhitespace(raw);
            if (!seenRaw.Add(raw)) return;
            var report = ParseSingle(raw);
            if (report is null || string.IsNullOrEmpty(report.StationIcao)) return;
            // Keep latest (first seen wins since messages are ordered by timestamp in the caller)
            if (seenIcao.Add(report.StationIcao))
                reports.Add(report);
        }

        // Pass 1: terminated blocks (= suffix) — handles multi-line METARs
        foreach (Match m in MetarTerminatedRegex().Matches(text))
            TryAdd(m.Value);

        // Pass 2: line-by-line — catches non-terminated METARs and those missed by pass 1
        foreach (Match m in MetarLineRegex().Matches(text))
            TryAdd(m.Value);

        return reports;
    }

    private static string NormalizeWhitespace(string s) =>
        Regex.Replace(s.Trim(), @"\s+", " ");

    private static MetarReport? ParseSingle(string raw)
    {
        var cleaned = raw.TrimEnd('=').Trim();
        var tokens = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length < 4) return null;

        var i = 0;

        // METAR or SPECI
        var isSpeci = tokens[i].Equals("SPECI", StringComparison.OrdinalIgnoreCase);
        i++;

        // Optional correction marker
        if (i < tokens.Length && tokens[i].Equals("COR", StringComparison.OrdinalIgnoreCase)) i++;

        // Station ICAO (4 uppercase letters)
        if (i >= tokens.Length) return null;
        var station = tokens[i++].ToUpperInvariant();

        // Observation time: DDHHMMz
        DateTime? obsTime = null;
        if (i < tokens.Length)
        {
            obsTime = ParseObsTime(tokens[i]);
            if (obsTime.HasValue) i++;
        }

        // Optional: AUTO, NIL, COR
        while (i < tokens.Length && tokens[i] is "AUTO" or "NIL" or "COR") i++;

        // Wind
        MetarWind? wind = null;
        if (i < tokens.Length && WindRegex().IsMatch(tokens[i]))
        {
            wind = ParseWind(tokens[i++]);
            // Skip variable wind sector: e.g. 200V300
            if (i < tokens.Length && VarWindSectorRegex().IsMatch(tokens[i])) i++;
        }

        // Visibility
        int? visMeters = null;
        var cavok = false;
        if (i < tokens.Length)
        {
            if (tokens[i].Equals("CAVOK", StringComparison.OrdinalIgnoreCase))
            {
                cavok = true;
                visMeters = 10000;
                i++;
            }
            else if (tokens[i] == "9999")
            {
                visMeters = 9999;
                i++;
                if (i < tokens.Length && DirVisRegex().IsMatch(tokens[i])) i++;
            }
            else if (tokens[i].Length == 4 && int.TryParse(tokens[i], out var v))
            {
                visMeters = v;
                i++;
                if (i < tokens.Length && DirVisRegex().IsMatch(tokens[i])) i++;
            }
        }

        // RVR groups (R28L/0800U)
        while (i < tokens.Length && RvrRegex().IsMatch(tokens[i])) i++;

        // Present weather
        var weather = new List<string>();
        if (!cavok)
        {
            while (i < tokens.Length && WeatherRegex().IsMatch(tokens[i]))
                weather.Add(tokens[i++]);
        }

        // Cloud groups
        var clouds = new List<MetarCloud>();
        if (!cavok)
        {
            while (i < tokens.Length)
            {
                var t = tokens[i];
                if (t is "SKC" or "NCD" or "NSC" or "CLR")
                {
                    clouds.Add(new MetarCloud { Coverage = t });
                    i++;
                }
                else if (CloudRegex().IsMatch(t))
                {
                    var c = ParseCloud(t);
                    if (c is null) break;
                    clouds.Add(c);
                    i++;
                }
                else break;
            }
        }

        // Temperature / dew point: 12/04, M03/M10
        double? temp = null, dew = null;
        if (i < tokens.Length && TempDewRegex().IsMatch(tokens[i]))
            (temp, dew) = ParseTempDew(tokens[i++]);

        // QNH: Q1018 (hPa) or A2992 (inHg×100)
        int? qnh = null;
        if (i < tokens.Length)
        {
            if (tokens[i].StartsWith('Q') && int.TryParse(tokens[i].AsSpan(1), out var q))
            { qnh = q; i++; }
            else if (tokens[i].StartsWith('A') && int.TryParse(tokens[i].AsSpan(1), out var a))
            { qnh = (int)Math.Round(a / 100.0 * 33.8639); i++; }
        }

        // Everything remaining is the trend / remark section
        string? trend = null;
        if (i < tokens.Length)
        {
            var t = string.Join(" ", tokens.Skip(i)).TrimEnd('=').Trim();
            if (!string.IsNullOrEmpty(t)) trend = t;
        }

        return new MetarReport
        {
            RawText = cleaned,
            IsSpeci = isSpeci,
            StationIcao = station,
            ObservationTime = obsTime,
            Wind = wind,
            VisibilityMeters = visMeters,
            Cavok = cavok,
            Clouds = clouds,
            WeatherGroups = weather,
            Temperature = temp,
            DewPoint = dew,
            Qnh = qnh,
            Trend = trend
        };
    }

    private static DateTime? ParseObsTime(string token)
    {
        if (token.Length != 7 || !token.EndsWith("Z", StringComparison.OrdinalIgnoreCase)) return null;
        if (!int.TryParse(token.AsSpan(0, 2), out var day)) return null;
        if (!int.TryParse(token.AsSpan(2, 2), out var hour)) return null;
        if (!int.TryParse(token.AsSpan(4, 2), out var min)) return null;
        var now = DateTime.UtcNow;
        try { return new DateTime(now.Year, now.Month, day, hour, min, 0, DateTimeKind.Utc); }
        catch { return null; }
    }

    private static MetarWind? ParseWind(string token)
    {
        var m = WindRegex().Match(token);
        if (!m.Success) return null;

        int? dir = m.Groups["dir"].Value.Equals("VRB", StringComparison.OrdinalIgnoreCase)
            ? null
            : int.Parse(m.Groups["dir"].Value);

        var speed = int.Parse(m.Groups["speed"].Value);
        int? gust = m.Groups["gust"].Success ? int.Parse(m.Groups["gust"].Value) : null;

        // Convert non-KT units to knots
        var unit = m.Groups["unit"].Value.ToUpperInvariant();
        if (unit == "MPS")
        {
            speed = (int)Math.Round(speed * 1.94384);
            if (gust.HasValue) gust = (int)Math.Round(gust.Value * 1.94384);
        }
        else if (unit == "KMH")
        {
            speed = (int)Math.Round(speed * 0.539957);
            if (gust.HasValue) gust = (int)Math.Round(gust.Value * 0.539957);
        }

        return new MetarWind { DirectionDegrees = dir, SpeedKnots = speed, GustKnots = gust };
    }

    private static MetarCloud? ParseCloud(string token)
    {
        var m = CloudRegex().Match(token);
        if (!m.Success) return null;
        int? alt = int.TryParse(m.Groups["alt"].Value, out var a) ? a * 100 : null;
        return new MetarCloud
        {
            Coverage = m.Groups["cov"].Value.ToUpperInvariant(),
            AltitudeFt = alt,
            Type = m.Groups["type"].Success && m.Groups["type"].Length > 0
                ? m.Groups["type"].Value.ToUpperInvariant()
                : null
        };
    }

    private static (double? temp, double? dew) ParseTempDew(string token)
    {
        var slash = token.IndexOf('/');
        if (slash < 0) return (null, null);
        return (ParseTempValue(token[..slash]), ParseTempValue(token[(slash + 1)..]));
    }

    private static double? ParseTempValue(string s)
    {
        if (s is "//" or "XX" or "") return null;
        var neg = s.StartsWith('M') || s.StartsWith('m');
        if (neg && double.TryParse(s.AsSpan(1), out var n)) return -n;
        if (double.TryParse(s, out var p)) return p;
        return null;
    }
}
