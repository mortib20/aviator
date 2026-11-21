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

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await HandleClientAsync(onReceive, tcpClient, cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        logger.LogError(ex, "Unhandled error in TCP client handler");
                    }
                }, cancellationToken);
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
        using var loggerScope = logger.BeginScope(EndPoint);
        await using var stream = client.GetStream();

        var remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
        logger.LogInformation("Client connected from {RemoteEndPoint}", remoteEndPoint);

        try
        {
            // TODO No message framing: each ReadAsync chunk (up to 8 KB) is handed on as one
            // message, but TCP is a byte stream - a chunk can hold several JSON messages or
            // only part of one (large dumpvdl2 frames, Nagle, slow links), which then fails to
            // parse. Split on '\n' instead (e.g. PipeReader + SequenceReader.TryReadTo) and keep
            // the remainder for the next read; TcpOutput already terminates every message with
            // '\n'. Unused for now: the deployed config reads UDP only.
            while (true)
            {
                var buffer = new byte[8192];
                var length = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);

                if (length == 0)
                {
                    logger.LogInformation("Client disconnected from {RemoteEndPoint}", remoteEndPoint);
                    client.Close();
                    break;
                }

                await handler.Invoke(buffer[..length], cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            client.Close();
        }
    }

    public override string ToString()
    {
        return $"{nameof(TcpInput)}/{host}:{port}";
    }
}
