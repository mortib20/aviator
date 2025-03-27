using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Aviator.Network.Output;

public sealed class UdpOutput(string host, int port, ILogger<UdpOutput> logger) : IOutput
{
    public string EndPoint { get; init; } = $"{host}:{port}";
    private IPEndPoint? _ipEndPoint;

    public async ValueTask WriteAsync(byte[] buffer, CancellationToken cancellationToken = default)
    {
        try
        {
            _ipEndPoint ??= new IPEndPoint((await Dns.GetHostAddressesAsync(host, cancellationToken).ConfigureAwait(false))[0], port);

            using var udpClient = new UdpClient();

            await udpClient.SendAsync(buffer, buffer.Length, _ipEndPoint).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "We call it unhandled error.");
        } // TODO maybe add exception handling here for if the ip of the service changes
    }
}