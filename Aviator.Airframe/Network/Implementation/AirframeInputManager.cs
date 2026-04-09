using System.Threading.Channels;
using Aviator.Network.Input;
using Microsoft.Extensions.Logging;

namespace Aviator.Airframe.Network.Implementation;

public class AirframeInputManager(ILogger<AirframeInputManager> logger, IInput input) : IAirframeInputManager
{
    private const int MinBytes = 128;

    private readonly Channel<byte[]> _channel = Channel.CreateUnbounded<byte[]>();

    public ChannelReader<byte[]> ChannelReader => _channel.Reader;
    
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Start Input on {Endpoint}", input.ToString());

        return input.ReceiveAsync(OnReceive, cancellationToken).WaitAsync(cancellationToken);
    }

    private async Task OnReceive(byte[] bytes, CancellationToken cancellationToken = default)
    {
        if (bytes.Length < MinBytes)
        {
            logger.LogWarning("Received payload to small...");
            return;
        }
        
        await _channel.Writer.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
    }
}