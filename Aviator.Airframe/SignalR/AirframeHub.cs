// using Microsoft.AspNetCore.SignalR;
//
// namespace Aviator.Acars;
//
// public class AirframeHub(AcarsPositionState acarsPositionState) : Hub
// {
//     public async Task<List<Position>> SubscribeToPositions()
//     {
//         await Groups.AddToGroupAsync(Context.ConnectionId, "Position").ConfigureAwait(false);
//
//         return acarsPositionState.Positions;
//     }
// }


using Microsoft.AspNetCore.SignalR;

namespace Aviator.Airframe.SignalR;

public class AirframeHub : Hub
{
    //
}