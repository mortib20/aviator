using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Aviator.Global.Service;

public abstract class AviatorBackgroundService(ILogger logger) : BackgroundService
{
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting {This}...", GetType().Name);
        await base.StartAsync(cancellationToken).WaitAsync(cancellationToken);
    }
    
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping {This}...", GetType().Name);
        await base.StopAsync(cancellationToken).WaitAsync(cancellationToken);
    }
}