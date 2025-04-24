using System.Text.Json.Nodes;

namespace Aviator.Acars.Handlers.PositionStuff;

public class Position
{
    public string Reg { get; set; }
    public decimal Lat { get; set; }
    public decimal Lon { get; set; }
    public int Alt { get; set; }
    public DateTimeOffset ReportTime = DateTimeOffset.Now;

    public static bool HasAdscPosition(JsonNode json)
    {
        var tags = json["vdl2"]?["avlc"]?["acars"]?["arinc622"]?["adsc"]?["tags"]?.AsArray();
        return tags?.OfType<JsonObject>().FirstOrDefault(s => s.ContainsKey("basic_report")) is not null;
    }
    
    public static Position FromAcarsFrame(JsonNode json)
    {
        var arinc = json["vdl2"]["avlc"]["acars"]["arinc622"];
        var tags = arinc?["adsc"]?["tags"]?.AsArray();
        var addr = arinc?["air_addr"];
        var basic_report = tags?.AsArray().OfType<JsonObject>()
            .SelectMany(tag => tag)
            .Where(kvp => kvp.Key == "basic_report")
            .Select(kvp => kvp.Value as JsonObject)
            .FirstOrDefault();;

        return new Position
        {
            Reg = addr.GetValue<string>(),
            Lat = basic_report["lat"].GetValue<decimal>(),
            Lon = basic_report["lon"].GetValue<decimal>(),
            Alt = basic_report["alt"].GetValue<int>(),
            ReportTime = DateTimeOffset.Now
        };
    }
}