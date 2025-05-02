using Aviator.Acars.Frames;
using Aviator.Network.Output;
using Microsoft.Extensions.Logging;

namespace Aviator.Acars.Network.Implementation;

public class AirframeOutputManager(ILogger<AirframeOutputManager> logger, Dictionary<FrameType, List<IOutput>> outputs) : IAirframeOutputManager
{
    public async Task SendToOutputOfTypeAsync(FrameType frameType, byte[] buffer, CancellationToken cancellationToken = default)
    {
        if (!outputs.TryGetValue(frameType, out var outputList))
        {
            logger.LogWarning("No output defined for {SourceType}", frameType);
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
            logger.LogError(e, "Error occured while trying to send to output of {AcarsType}", frameType);
        }
    }
}