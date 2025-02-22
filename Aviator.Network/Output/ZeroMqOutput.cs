using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;

namespace Aviator.Network.Output;

public class ZeroMqOutput(string host, int port, ILogger<ZeroMqOutput> logger) : IOutput
{
    public string EndPoint { get; init; } = $"{host}:{port}";
    public ValueTask WriteAsync(byte[] buffer, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = new RequestSocket($">tcp://{EndPoint}");
            client.SendFrame(buffer);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send over ZeroMQ");
        }
        return ValueTask.CompletedTask;
    }
}