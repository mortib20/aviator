using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace Aviator.Network.Output.Implementation;

public sealed class TcpOutput : IOutput, IDisposable
{
    private static readonly TimeSpan ErrorTimeout = TimeSpan.FromSeconds(4);
    private readonly Timer _connectionTimer = new(ErrorTimeout);

    private TcpClient? _client;
    private bool _connected;
    private readonly string _host;
    private readonly int _port;
    private readonly ILogger<TcpOutput> _logger;

    public TcpOutput(string host, int port, ILogger<TcpOutput> logger)
    {
        _host = host;
        _port = port;
        _logger = logger;
        EndPoint = $"{host}:{port}";

        _connectionTimer.Elapsed += async (sender, args) =>
        {
            if (_connected)
            {
                return;
            }

            try
            {
                logger.LogInformation("Connecting to {A}", EndPoint);
                _client = new TcpClient();
                await _client.ConnectAsync(host, port);
                _connected = true;
            }
            catch (Exception ex)
            {
                _connected = false;
                _logger.LogWarning(ex, "Client failed to connect or got disconnected from {Host}:{Port}, waiting for {ErrorTimeout} seconds!", _host, _port, ErrorTimeout.TotalSeconds);
            }
        };

        _connectionTimer.Start();
    }

    public void Dispose()
    {
        Dispose(true);
    }

    public string EndPoint { get; init; }

    public async ValueTask WriteAsync(byte[] buffer, CancellationToken cancellationToken = default)
    {
        if (_client is null || !_connected)
        {
            return;
        }

        try
        {
            await _client.GetStream().WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is IOException or SocketException)
        {
            _connected = false;
            _logger.LogWarning(ex, "Client failed to connect or got disconnected from {Host}:{Port}, waiting for {ErrorTimeout} seconds!", _host, _port, ErrorTimeout.TotalSeconds);
        }
    }

    private void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        _client?.Dispose();
    }
}