using System.Net;
using System.Net.Sockets;

namespace Aviator.Network.Input;

public class UdpInput(string host, int port) : IInput
{
    public string EndPoint { get; init; } = $"{host}:{port}";

    public async Task ReceiveAsync(InputHandler onReceive, CancellationToken cancellationToken = default)
    {
        using var udpClient = new UdpClient(new IPEndPoint(IPAddress.Parse(host), port));
        udpClient.Client.ReceiveBufferSize = ushort.MaxValue;
        udpClient.DontFragment = false;

        while (!cancellationToken.IsCancellationRequested)
        {
            var datagram = await udpClient.ReceiveAsync(cancellationToken).ConfigureAwait(false);
            await onReceive.Invoke(datagram.Buffer, cancellationToken);
        }
    }

    public override string ToString()
    {
        return $"{nameof(UdpClient)}/{host}:{port}";
    }
}