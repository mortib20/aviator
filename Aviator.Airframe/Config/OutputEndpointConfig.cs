using Aviator.Acars.Entities;
using Aviator.Network.Config;

namespace Aviator.Acars.Config;

public class OutputEndpointConfig : EndpointConfig
{
    public List<SourceType> Types { get; set; } = [];
}