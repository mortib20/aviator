namespace Aviator.Airframe.Metrics;

public interface IAirframeMetric
{
    Task WriteAirframeAsync(Frames.Entities.Airframe airframe, CancellationToken cancellationToken = default);
}