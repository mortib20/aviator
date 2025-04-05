using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Aviator.Network.Input.Implementation;

public class TcpInput(ILogger<IInput> logger, string host, int port) : IInput
{
    private const int MaxBufferSize = ushort.MaxValue;

    public string EndPoint { get; init; } = $"{host}:{port}";

    public async Task ReceiveAsync(InputHandler onReceive, CancellationToken cancellationToken = default)
    {
        using var tcpListener = new TcpListener(IPAddress.Parse(host), port);
        
        tcpListener.Start();
        
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var tcpClient = await tcpListener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);

                _ = Task.Run(async () => await HandleClientAsync(onReceive, tcpClient, cancellationToken).ConfigureAwait(false), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Ignore
            }
        }
        
        tcpListener.Stop();
    }

    private async Task HandleClientAsync(InputHandler handler, TcpClient client,
        CancellationToken cancellationToken = default)
    {
        var loggerScope = logger.BeginScope(EndPoint);
        await using var stream = client.GetStream();

        var remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
        logger.LogInformation("Client connected from {RemoteEndPoint}", remoteEndPoint);

        while (client.Connected)
        {
            var buffer = new byte[MaxBufferSize];
            var length = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);

            if (length == 0 || !client.Connected)
            {
                logger.LogInformation("Client disconnected from {RemoteEndPoint}", remoteEndPoint);
                client.Close();
                break;
            }

            await handler.Invoke(buffer[..length].ToArray(), cancellationToken).ConfigureAwait(false);
        }

        loggerScope?.Dispose();
    }

    public override string ToString()
    {
        return $"{nameof(TcpInput)}/{host}:{port}";
    }
}