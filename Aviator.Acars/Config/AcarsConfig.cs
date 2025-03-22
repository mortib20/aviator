using Aviator.Network.Config;

namespace Aviator.Acars.Config;

public class AcarsConfig
{
    public const string Section = "Acars";
    public EndpointConfig? Input { get; init; }
    public List<OutputEndpointConfig> Outputs { get; set; } = [];
    public MongoDbConfig? MongoDb { get; init; }
}