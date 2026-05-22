using Aviator.Network.Config;

namespace Aviator.Airframe.Config;

public class AirframeConfig
{
    public const string Section = "Airframe";
    public required EndpointConfig Input { get; init; }
    public required List<OutputEndpointConfig> Outputs { get; set; } = [];
}