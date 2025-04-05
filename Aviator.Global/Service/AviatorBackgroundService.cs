using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Aviator.Global.Service;

public class AviatorBackgroundService(ILogger logger) : BackgroundService
{
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting {This}...", this);
        await base.StartAsync(cancellationToken).WaitAsync(cancellationToken);
    }
    
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping {This}...", this);
        await base.StopAsync(cancellationToken).WaitAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException("We should never be here!");
    }
}