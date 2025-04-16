using Aviator.Acars.Entities;

namespace Aviator.Acars.Network;

public interface IAcarsOutputManager
{
    Task SendToOutputOfTypeAsync(SourceType sourceType, byte[] buffer, CancellationToken cancellationToken = default);
}