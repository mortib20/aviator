using Aviator.Acars.Entities;

namespace Aviator.Acars.Metrics.Implementation;

public class AirframeMetrics(ICollection<IAirframeMetrics> metricsList) : IAirframeMetrics
{
    public async Task IncreaseAsync(AirFrame frame, CancellationToken cancellationToken = default)
    {
        foreach (var metrics in metricsList)
        {
            await metrics.IncreaseAsync(frame, cancellationToken);
        }
    }
}