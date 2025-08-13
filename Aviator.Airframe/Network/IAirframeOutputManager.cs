using Aviator.Airframe.Frames;

namespace Aviator.Airframe.Network;

public interface IAirframeOutputManager
{
    Task SendToOutputsOfFrameTypeAsync(FrameType frameType, byte[] buffer, CancellationToken cancellationToken = default);
}