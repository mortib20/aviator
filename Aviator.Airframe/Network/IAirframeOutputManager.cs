using Aviator.Acars.Entities;
using Aviator.Acars.Frames;

namespace Aviator.Acars.Network;

public interface IAirframeOutputManager
{
    Task SendToOutputOfTypeAsync(FrameType frameType, byte[] buffer, CancellationToken cancellationToken = default);
}