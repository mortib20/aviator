using System.Text.Json.Nodes;

namespace Aviator.Acars.Handlers.PositionStuff;

public class Position
{
    public string Reg { get; set; }
    public decimal Lat { get; set; }
    public decimal Lon { get; set; }
    public int Alt { get; set; }
    public DateTimeOffset DateTime { get; set; } = DateTimeOffset.Now;

    public static bool HasAdscPosition(JsonNode json)
    {
        var tags = json["vdl2"]?["avlc"]?["acars"]?["arinc622"]?["adsc"]?["tags"]?.AsArray().OfType<JsonObject>();
        return tags?.FirstOrDefault(s => s.ContainsKey("basic_report")) is not null;
    }

    public static bool HasXidPosition(JsonNode json)
    {
        var xid = json["vdl2"]?["avlc"]?["xid"];
        var vdlParams = xid?["vdl_params"]?.AsArray().OfType<JsonObject>()
            .ToDictionary(k => k["name"].GetValue<string>(), v => v["value"]);
        return vdlParams?.ContainsKey("ac_location") is not false;
    }
    
    public static Position FromAcarsAdscFrame(JsonNode json)
    {
        var arinc = json["vdl2"]["avlc"]["acars"]["arinc622"];
        var tags = arinc?["adsc"]?["tags"]?.AsArray();
        var addr = json["vdl2"]?["avlc"]?["src"]?["addr"]?.GetValue<string>();
        var basic_report = tags?.AsArray().OfType<JsonObject>()
            .SelectMany(tag => tag)
            .Where(kvp => kvp.Key == "basic_report")
            .Select(kvp => kvp.Value as JsonObject)
            .FirstOrDefault();

        return new Position
        {
            Reg = addr,
            Lat = basic_report["lat"].GetValue<decimal>(),
            Lon = basic_report["lon"].GetValue<decimal>(),
            Alt = basic_report["alt"].GetValue<int>(),
            DateTime = DateTimeOffset.Now
        };
    }
    
    public static Position FromXidAcLocationFrame(JsonNode json)
    {
        var xid = json["vdl2"]?["avlc"]?["xid"];
        var addr = json["vdl2"]?["avlc"]?["src"]?["addr"]?.GetValue<string>();
        var vdlParams = xid?["vdl_params"]?.AsArray().OfType<JsonObject>()
            .ToDictionary(k => k["name"].GetValue<string>(), v => v["value"]);
        var acLocation = vdlParams["ac_location"].AsObject();
        
        return new Position
        {
            Reg = addr,
            Lat = acLocation["loc"]["lat"].GetValue<decimal>(),
            Lon = acLocation["loc"]["lon"].GetValue<decimal>(),
            Alt = acLocation["alt"].GetValue<int>(),
            DateTime = DateTimeOffset.Now
        };
    }
}