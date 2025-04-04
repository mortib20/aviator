using Aviator.Network.Input;
using Microsoft.Extensions.Logging;

namespace Aviator.Acars.Network.Implementation;

public class AcarsInputManager(ILogger<AcarsInputManager> logger, IInput input) : IAcarsInputManager
{
    public async Task StartInputAsync(InputHandler onReceivedAsync, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Start Input on {Endpoint}", input.EndPoint);

        await input.ReceiveAsync(onReceivedAsync, cancellationToken).ConfigureAwait(false);
    }
}