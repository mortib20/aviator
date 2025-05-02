using Aviator.Network.Config;

namespace Aviator.Airframe.Config;

public class AcarsConfig
{
    public const string Section = "Acars";
    public required EndpointConfig Input { get; init; }
    public required List<OutputEndpointConfig> Outputs { get; set; } = [];
    public MongoDbConfig? MongoDb { get; init; }
}