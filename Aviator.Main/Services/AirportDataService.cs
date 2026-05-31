using System.Globalization;
using Microsoft.Extensions.Logging;

namespace Aviator.Main.Services;

public sealed record AirportInfo(string Icao, string Name, double Lat, double Lon);

public sealed class AirportDataService(IHttpClientFactory httpFactory, ILogger<AirportDataService> logger)
{
    private static readonly HashSet<string> AllowedTypes =
        new(StringComparer.OrdinalIgnoreCase) { "large_airport", "medium_airport" };

    private const string CsvUrl =
        "https://davidmegginson.github.io/ourairports-data/airports.csv";

    private Dictionary<string, AirportInfo>? _airports;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<AirportInfo?> GetByIcaoAsync(string icao, CancellationToken ct = default)
    {
        var airports = await EnsureLoadedAsync(ct);
        airports.TryGetValue(icao.ToUpperInvariant(), out var info);
        return info;
    }

    public async Task<IReadOnlyDictionary<string, AirportInfo>> GetAllAsync(CancellationToken ct = default) =>
        await EnsureLoadedAsync(ct);

    private async Task<Dictionary<string, AirportInfo>> EnsureLoadedAsync(CancellationToken ct)
    {
        if (_airports is not null) return _airports;

        await _lock.WaitAsync(ct);
        try
        {
            if (_airports is not null) return _airports;
            _airports = await FetchAsync(ct);
        }
        finally
        {
            _lock.Release();
        }

        return _airports;
    }

    private async Task<Dictionary<string, AirportInfo>> FetchAsync(CancellationToken ct)
    {
        var result = new Dictionary<string, AirportInfo>(StringComparer.OrdinalIgnoreCase);
        try
        {
            var client = httpFactory.CreateClient("airports");
            var csv = await client.GetStringAsync(CsvUrl, ct);
            var lines = csv.Split('\n');
            if (lines.Length < 2) return result;

            var header = SplitCsvLine(lines[0]);
            int Idx(string name) => Array.FindIndex(header, h => h.Equals(name, StringComparison.OrdinalIgnoreCase));

            var iIdent = Idx("ident");
            var iType  = Idx("type");
            var iName  = Idx("name");
            var iLat   = Idx("latitude_deg");
            var iLon   = Idx("longitude_deg");

            if (iIdent < 0 || iLat < 0 || iLon < 0) return result;

            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var cols = SplitCsvLine(line);
                var max = Math.Max(iIdent, Math.Max(iLat, iLon));
                if (cols.Length <= max) continue;

                var type = iType >= 0 && iType < cols.Length ? cols[iType] : "";
                if (!AllowedTypes.Contains(type)) continue;

                var icao = cols[iIdent];
                if (string.IsNullOrEmpty(icao) || icao.Length != 4) continue;
                if (!double.TryParse(cols[iLat], NumberStyles.Float, CultureInfo.InvariantCulture, out var lat)) continue;
                if (!double.TryParse(cols[iLon], NumberStyles.Float, CultureInfo.InvariantCulture, out var lon)) continue;

                var name = iName >= 0 && iName < cols.Length && !string.IsNullOrEmpty(cols[iName])
                    ? cols[iName]
                    : icao;

                result[icao] = new AirportInfo(icao, name, lat, lon);
            }

            logger.LogInformation("Loaded {Count} airports", result.Count);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not load OurAirports data");
        }

        return result;
    }

    private static string[] SplitCsvLine(string line)
    {
        var fields = new List<string>();
        var buf = new System.Text.StringBuilder();
        var inQuotes = false;

        foreach (var ch in line)
        {
            switch (ch)
            {
                case '"': inQuotes = !inQuotes; break;
                case ',' when !inQuotes:
                    fields.Add(buf.ToString());
                    buf.Clear();
                    break;
                default: buf.Append(ch); break;
            }
        }
        fields.Add(buf.ToString());
        return [.. fields];
    }
}
