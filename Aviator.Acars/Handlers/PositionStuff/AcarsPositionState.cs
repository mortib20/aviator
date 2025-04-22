using Microsoft.AspNetCore.SignalR;

namespace Aviator.Acars.Handlers.PositionStuff;

public class AcarsPositionState(IHubContext<AcarsHub> acarsHub)
{
    public List<Position> Positions { get; } = [];

    public async Task AddPositionAsync(Position position, CancellationToken cancellationToken = default)
    {
        Positions.Add(position);

        await acarsHub.Clients.Group("Positions").SendAsync("Positions", Positions, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}