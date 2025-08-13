using Aviator.Airframe.Frames;
using Aviator.Network.Config;

namespace Aviator.Airframe.Config;

public class OutputEndpointConfig : EndpointConfig
{
    public List<FrameType> Types { get; set; } = [];
}