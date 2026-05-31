using Aviator.Airframe.Database;
using Aviator.Main.Decoders.Metar;
using Microsoft.EntityFrameworkCore;

namespace Aviator.Main.Services;

// ── DTOs ─────────────────────────────────────────────────────────────────────

public sealed record TrackPoint(double Lat, double Lon, int? AltFt, DateTimeOffset Ts);

public sealed record AircraftDto(
    string Icao,
    string? Registration,
    double LastLat,
    double LastLon,
    int? LastAltFt,
    DateTimeOffset LastSeen,
    IReadOnlyList<TrackPoint> Track);

public sealed record MetarStationDto(
    string Icao,
    string Name,
    double Lat,
    double Lon,
    double? Temperature,
    double? DewPoint,
    int? WindDir,
    int? WindSpeedKt,
    int? WindGustKt,
    int? Qnh,
    int? VisibilityMeters,
    bool Cavok,
    string? Trend,
    DateTimeOffset Timestamp);

public sealed record MapDataDto(
    IReadOnlyList<AircraftDto> Aircraft,
    IReadOnlyList<MetarStationDto> MetarStations);

// ── Service ───────────────────────────────────────────────────────────────────

public sealed class MapQueryService(
    IDbContextFactory<AviatorDbContext> dbFactory,
    AirportDataService airports)
{
    private static readonly TimeSpan MaxRange = TimeSpan.FromDays(7);

    public async Task<MapDataDto> GetMapDataAsync(
        DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default)
    {
        // Clamp to prevent runaway queries
        if (to - from > MaxRange) from = to - MaxRange;

        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var aircraft  = await QueryAircraftAsync(db, from, to, ct);
        var metarStns = await QueryMetarStationsAsync(db, from, to, ct);

        return new MapDataDto(aircraft, metarStns);
    }

    private static async Task<List<AircraftDto>> QueryAircraftAsync(
        AviatorDbContext db, DateTimeOffset from, DateTimeOffset to, CancellationToken ct)
    {
        var rows = await db.Airframes
            .Where(a => a.Timestamp >= from && a.Timestamp <= to
                     && a.Latitude.HasValue && a.Longitude.HasValue
                     && a.RealPosition == true)
            .OrderBy(a => a.Timestamp)
            .Select(a => new
            {
                a.Icao,
                a.AcarsRegistration,
                a.Latitude,
                a.Longitude,
                a.Altitude,
                a.Timestamp
            })
            .ToListAsync(ct);

        var result = new List<AircraftDto>();

        foreach (var group in rows.Where(r => !string.IsNullOrEmpty(r.Icao)).GroupBy(r => r.Icao!))
        {
            var sorted = group.OrderBy(r => r.Timestamp).ToList();
            var last   = sorted[^1];

            var track = sorted
                .Select(p => new TrackPoint(
                    (double)p.Latitude!.Value,
                    (double)p.Longitude!.Value,
                    p.Altitude.HasValue ? (int)p.Altitude.Value : null,
                    p.Timestamp))
                .ToList();

            result.Add(new AircraftDto(
                Icao:         group.Key,
                Registration: last.AcarsRegistration,
                LastLat:      (double)last.Latitude!.Value,
                LastLon:      (double)last.Longitude!.Value,
                LastAltFt:    last.Altitude.HasValue ? (int)last.Altitude.Value : null,
                LastSeen:     last.Timestamp,
                Track:        track));
        }

        return result;
    }

    private async Task<List<MetarStationDto>> QueryMetarStationsAsync(
        AviatorDbContext db, DateTimeOffset from, DateTimeOffset to, CancellationToken ct)
    {
        var rows = await db.Airframes
            .Where(a => a.Timestamp >= from && a.Timestamp <= to
                     && a.AcarsText != null
                     && (EF.Functions.Like(a.AcarsText, "%METAR%") ||
                         EF.Functions.Like(a.AcarsText, "%SPECI%")))
            .OrderBy(a => a.Timestamp)
            .Select(a => new { a.AcarsText, a.Timestamp })
            .ToListAsync(ct);

        // Decode and keep the latest report per station
        var byStation = new Dictionary<string, (MetarReport Report, DateTimeOffset Ts)>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            if (row.AcarsText is null) continue;
            foreach (var report in MetarParser.ExtractAndParse(row.AcarsText))
            {
                if (string.IsNullOrEmpty(report.StationIcao)) continue;
                if (!byStation.TryGetValue(report.StationIcao, out var existing) ||
                    row.Timestamp > existing.Ts)
                {
                    byStation[report.StationIcao] = (report, row.Timestamp);
                }
            }
        }

        var airportLookup = await airports.GetAllAsync(ct);
        var result = new List<MetarStationDto>();

        foreach (var (icao, (report, ts)) in byStation)
        {
            if (!airportLookup.TryGetValue(icao, out var airport)) continue;

            result.Add(new MetarStationDto(
                Icao:             icao,
                Name:             airport.Name,
                Lat:              airport.Lat,
                Lon:              airport.Lon,
                Temperature:      report.Temperature,
                DewPoint:         report.DewPoint,
                WindDir:          report.Wind?.DirectionDegrees,
                WindSpeedKt:      report.Wind?.SpeedKnots,
                WindGustKt:       report.Wind?.GustKnots,
                Qnh:              report.Qnh,
                VisibilityMeters: report.VisibilityMeters,
                Cavok:            report.Cavok,
                Trend:            report.Trend,
                Timestamp:        ts));
        }

        return result;
    }
}
