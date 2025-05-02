using System.Threading.Channels;

namespace Aviator.Acars.Network;

public interface IAirframeInputManager
{
    ChannelReader<byte[]> ChannelReader { get; }
    
    Task StartAsync(CancellationToken cancellationToken = default);
}