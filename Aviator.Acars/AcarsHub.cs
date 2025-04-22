using Aviator.Acars.Handlers.PositionStuff;
using Microsoft.AspNetCore.SignalR;

namespace Aviator.Acars;

public class AcarsHub(AcarsPositionState acarsPositionState) : Hub
{
    public async Task SubscribeToPositions()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Positions").ConfigureAwait(false);

        var positions = acarsPositionState.Positions;

        await Clients.Caller.SendAsync("Positions", positions).ConfigureAwait(false);
    }
}