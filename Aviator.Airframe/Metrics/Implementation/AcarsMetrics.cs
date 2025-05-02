using Aviator.Acars.Entities;

namespace Aviator.Acars.Metrics.Implementation;

public class AcarsMetrics(ICollection<IAcarsMetrics> metricsList) : IAcarsMetrics
{
    public async Task IncreaseAsync(AirFrame frame, CancellationToken cancellationToken = default)
    {
        foreach (var metrics in metricsList)
        {
            await metrics.IncreaseAsync(frame, cancellationToken);
        }
    }
}