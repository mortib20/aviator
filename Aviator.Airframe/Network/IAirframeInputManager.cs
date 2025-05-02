using System.Threading.Channels;

namespace Aviator.Airframe.Network;

public interface IAirframeInputManager
{
    ChannelReader<byte[]> ChannelReader { get; }
    
    Task StartAsync(CancellationToken cancellationToken = default);
}