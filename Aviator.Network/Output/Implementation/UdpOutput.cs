using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Aviator.Network.Output.Implementation;

public sealed class UdpOutput(string host, int port, ILogger<UdpOutput> logger) : IOutput, IDisposable
{
    public string EndPoint { get; init; } = $"{host}:{port}";
    private IPEndPoint? _ipEndPoint;
    private UdpClient? _udpClient;

    public async ValueTask WriteAsync(byte[] buffer, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_ipEndPoint is null)
            {
                var addresses = await Dns.GetHostAddressesAsync(host, cancellationToken).ConfigureAwait(false);
                _ipEndPoint = new IPEndPoint(addresses[0], port);
                _udpClient = new UdpClient();
            }

            await _udpClient!.SendAsync(buffer, buffer.Length, _ipEndPoint).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "We call it unhandled error.");
        }
    }

    public void Dispose()
    {
        _udpClient?.Dispose();
    }
}
