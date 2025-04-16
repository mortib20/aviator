using System.Text.Json.Nodes;

namespace Aviator.Acars.Entities;

public static class SourceTypeFinder
{
    public static SourceType? Detect(JsonNode json)
    {
        try
        {
            if (IsAero(json))
            {
                return SourceType.Aero;
            }

            if (IsAcars(json))
            {
                return SourceType.Acars;
            }

            if (IsHfdl(json))
            {
                return SourceType.Hfdl;
            }

            if (IsVdl2(json))
            {
                return SourceType.Vdl2;
            }

            if (IsIridium(json))
            {
                return SourceType.Iridium;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return null;
    }

    private static bool IsAero(JsonNode json)
    {
        return json?["app"]?["name"]?.ToString() == "JAERO";
    }

    private static bool IsAcars(JsonNode json)
    {
        return json?["app"]?["name"]?.ToString() == "acarsdec";
    }

    private static bool IsHfdl(JsonNode json)
    {
        return json?["hfdl"]?["app"]?["name"]?.ToString() == "dumphfdl";
    }

    private static bool IsVdl2(JsonNode json)
    {
        return json?["vdl2"]?["app"]?["name"]?.ToString() == "dumpvdl2";
    }

    private static bool IsIridium(JsonNode json)
    {
        return json?["app"]?["name"]?.ToString() == "iridium-toolkit";
    }
}