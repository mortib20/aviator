using Aviator.Adsb.Entities;

namespace Aviator.Adsb.Metrics;

public interface IAdsbMetrics
{
    public Task IncreaseAsync(AdsbStats adsbStats, CancellationToken cancellationToken = default);
}