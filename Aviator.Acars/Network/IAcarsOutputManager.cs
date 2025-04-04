using Aviator.Acars.Entities;

namespace Aviator.Acars.Network;

public interface IAcarsOutputManager
{
    Task WriteToTypeAsync(SourceType sourceType, byte[] buffer, CancellationToken cancellationToken = default);
}