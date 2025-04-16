using System.Text.Json.Nodes;
using Aviator.Acars.Entities;

namespace Aviator.Acars.Utils;

public static class AirframeParser
{
    public static bool TryParseBytesToJson(byte[] bytes, out JsonNode jsonAcars)
    {
        try
        {
            jsonAcars = JsonNode.Parse(bytes) ?? throw new InvalidOperationException();
        }
        catch (Exception)
        {
            jsonAcars = null!;
            return false;
        }
        
        return true;
    }
    
    public static bool TryGetSourceType(JsonNode jsonAcars, out SourceType? sourceType)
    {
        try
        {
            sourceType = SourceTypeFinder.Detect(jsonAcars) ?? throw new InvalidOperationException();
        }
        catch (Exception)
        {
            sourceType = null!;
            return false;
        }

        return true;
    }
}