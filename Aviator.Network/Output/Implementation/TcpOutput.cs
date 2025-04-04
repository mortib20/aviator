using System.Net.Sockets;
using System.Timers;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace Aviator.Network.Output.Implementation;

public sealed class TcpOutput : IOutput, IDisposable
{
    private static readonly TimeSpan ErrorTimeout = TimeSpan.FromSeconds(4);
    private readonly Timer _connectionTimer = new(ErrorTimeout);
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    private TcpClient? _client;
    private bool _connected;
    private readonly string _host;
    private readonly int _port;
    private readonly ILogger<TcpOutput> _logger;
    
    public string EndPoint { get; init; }
    
    public TcpOutput(string host, int port, ILogger<TcpOutput> logger)
    {
        _host = host;
        _port = port;
        _logger = logger;
        EndPoint = $"{host}:{port}";

        _connectionTimer.Elapsed += async (_, _) => await ConnectionChecker().ConfigureAwait(false);
        _connectionTimer.AutoReset = true;
        _connectionTimer.Start();
    }

    private async Task ConnectionChecker()
    {
        if (_connected)
        {
            return;
        }

        await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
        try
        {
            _client?.Dispose();
            _client = new TcpClient();

            _logger.LogInformation("Connecting to {EndPoint}", EndPoint);
            await _client.ConnectAsync(_host, _port);
            _connected = true;
        }
        catch (Exception ex)
        {
            HandleDisconnectError(ex);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public async ValueTask WriteAsync(byte[] buffer, CancellationToken cancellationToken = default)
    {
        if (_client is null || !_connected)
        {
            return;
        }

        await _semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await _client.GetStream().WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is IOException or SocketException)
        {
            HandleDisconnectError(ex);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private void HandleDisconnectError(Exception ex)
    {
        _connected = false;
        _logger.LogWarning(ex, "Client failed to connect or got disconnected from {Host}:{Port}, waiting for {ErrorTimeout} seconds, before try reconnecting!", _host, _port, ErrorTimeout.TotalSeconds);
    }

    public void Dispose()
    {
        Dispose(true);
    }

    private void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        _semaphoreSlim.Wait();
        try
        {
            _client?.Dispose();
            _connectionTimer.Stop();
            _connectionTimer.Dispose();
        }
        finally
        {
            _semaphoreSlim.Release();
            _semaphoreSlim.Dispose();   
        }
    }
}