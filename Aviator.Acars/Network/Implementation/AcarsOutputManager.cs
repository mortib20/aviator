using Aviator.Acars.Entities;
using Aviator.Network.Output;
using Microsoft.Extensions.Logging;

namespace Aviator.Acars.Network.Implementation;

public class AcarsOutputManager(ILogger<AcarsOutputManager> logger, Dictionary<SourceType, List<IOutput>> outputs) : IAcarsOutputManager
{
    public async Task SendToOutputOfTypeAsync(SourceType sourceType, byte[] buffer,
        CancellationToken cancellationToken = default)
    {
        if (!outputs.TryGetValue(sourceType, out var outputList))
        {
            logger.LogWarning("No output defined for {SourceType}", sourceType);
            return;
        }

        try
        {
            foreach (var output in outputList.ToList())
            {
                await output.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occured while trying to send to output of {AcarsType}", sourceType);
        }
    }
}