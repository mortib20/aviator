using Aviator.Adsb.Entities;

namespace Aviator.Adsb.Metrics;

public interface IAdsbMetrics
{
    public Task IncreaseAsync(Dictionary<string, string> adsbStats, CancellationToken cancellationToken = default);
}