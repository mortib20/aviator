using System.Threading.Channels;

namespace Aviator.Acars.Network;

public interface IAcarsInputManager
{
    ChannelReader<byte[]> ChannelReader { get; }
    
    Task StartAsync(CancellationToken cancellationToken = default);
}