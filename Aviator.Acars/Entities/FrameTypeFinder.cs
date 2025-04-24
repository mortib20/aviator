using System.Text.Json.Nodes;

namespace Aviator.Acars.Entities;

public static class FrameTypeFinder
{
    public static bool HasAcars(JsonNode json)
    {
        var vdl2 = json["vdl2"]?["avlc"]?["acars"] is not null;
        var hfdl = json["hfdl"]?["lpdu"]?["hfnpdu"]?["acars"] is not null;
        var jaero = json["isu"]?["acars"] is not null;
        var acars = json["text"] is not null;
        var iridium = json["acars"]?["text"] is not null;
        return vdl2 || hfdl || jaero || acars || iridium;
    }

    public static bool HasXid(JsonNode json)
    {
        var vdl2 = json["vdl2"]?["avlc"]?["xid"] is not null;
        return vdl2;
    }
}