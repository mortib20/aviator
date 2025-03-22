using System.Collections.ObjectModel;
using Aviator.Adsb.Entities;

namespace Aviator.Adsb.Metrics;

public class AdsbMetrics(Collection<IAdsbMetrics> metricsList) : IAdsbMetrics
{
    public async Task IncreaseAsync(AdsbStats adsbStats, CancellationToken cancellationToken = default)
    {
        foreach (var adsbMetricse in metricsList)
        {
            await adsbMetricse.IncreaseAsync(adsbStats, cancellationToken).ConfigureAwait(false);
        }
    }
}