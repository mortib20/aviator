using Aviator.Acars.Entities;
using Aviator.Network.Input;
using Aviator.Network.Output;
using Microsoft.Extensions.Logging;

namespace Aviator.Acars;

public class AcarsIoManager(ILogger<AcarsIoManager> logger, IInput input, Dictionary<SourceType, List<IOutput>> outputs)
{
    public async Task StartInputAsync(InputHandler onReceivedAsync, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Start Input on {Endpoint}", input.EndPoint);

        await input.ReceiveAsync(onReceivedAsync, cancellationToken).ConfigureAwait(false);
    }

    public async Task WriteToTypeAsync(SourceType sourceType, byte[] buffer,
        CancellationToken cancellationToken = default)
    {
        if (!outputs.TryGetValue(sourceType, out var outputList)) return;

        foreach (var output in outputList.ToList())
        {
            await output.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
        }
    }
}