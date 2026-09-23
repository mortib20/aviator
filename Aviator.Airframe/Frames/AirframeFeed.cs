using Microsoft.Extensions.Logging;

namespace Aviator.Airframe.Frames;

/// <summary>
/// In-process fan-out of decoded ACARS airframes. Blazor components subscribe here
/// directly instead of opening a SignalR client connection back to their own server.
/// </summary>
public sealed class AirframeFeed(ILogger<AirframeFeed> logger)
{
    public event Action<Entities.Airframe>? Received;

    public void Publish(Entities.Airframe airframe)
    {
        if (Received is not { } handlers) return;

        // Invoke individually so one faulty subscriber can't starve the others
        foreach (var handler in handlers.GetInvocationList().Cast<Action<Entities.Airframe>>())
        {
            try
            {
                handler(airframe);
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Airframe feed subscriber failed");
            }
        }
    }
}
