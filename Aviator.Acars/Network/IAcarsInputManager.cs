using Aviator.Network.Input;

namespace Aviator.Acars.Network;

public interface IAcarsInputManager
{
    Task StartInputAsync(InputHandler onReceivedAsync, CancellationToken cancellationToken = default);
}