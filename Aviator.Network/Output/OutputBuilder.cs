using Aviator.Network.Output.Implementation;
using Microsoft.Extensions.Logging;

namespace Aviator.Network.Output;

public class OutputBuilder(ILoggerFactory loggerFactory) : IBuilder<IOutput>
{
    public IOutput Create(Protocol protocol, string host, int port)
    {
        return protocol switch
        {
            Protocol.Tcp => CreateTcpOutput(host, port),
            Protocol.Udp => CreateUdpOutput(host, port),
            _ => throw new ArgumentOutOfRangeException(nameof(protocol), protocol, null)
        };
    }

    private TcpOutput CreateTcpOutput(string host, int port)
    {
        return new TcpOutput(host, port, loggerFactory.CreateLogger<TcpOutput>());
    }

    private UdpOutput CreateUdpOutput(string host, int port)
    {
        return new UdpOutput(host, port, loggerFactory.CreateLogger<UdpOutput>());
    }
}