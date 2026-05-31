using System.Globalization;
using Aviator.Main.Services;

namespace Aviator.Main.Api;

public static class MapEndpoints
{
    public static void MapMapApi(this WebApplication app)
    {
        app.MapGet("/api/map/data", async (
            string? from,
            string? to,
            MapQueryService query,
            CancellationToken ct) =>
        {
            if (!DateTimeOffset.TryParse(from, null, DateTimeStyles.RoundtripKind, out var fromDt))
                return Results.BadRequest("Invalid 'from' date");
            if (!DateTimeOffset.TryParse(to, null, DateTimeStyles.RoundtripKind, out var toDt))
                return Results.BadRequest("Invalid 'to' date");
            if (toDt <= fromDt)
                return Results.BadRequest("'to' must be after 'from'");

            var data = await query.GetMapDataAsync(fromDt, toDt, ct);
            return Results.Ok(data);
        });
    }
}
